using CollegePortal.API.Data;
using CollegePortal.API.DTOs.Profile;
using CollegePortal.API.DTOs.Entity;
using CollegePortal.API.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegePortal.API.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "STUDENT")]
    public class StudentController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public StudentController(CollegePortalDbContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

        /// <summary>Get student profile</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var student = await _context.Students.FindAsync(GetUserId())
                ?? throw new NotFoundException("Student not found");

            var profile = new StudentProfileDTO
            {
                Id = student.Id,
                RollNumber = student.RollNumber,
                Name = student.Name,
                Email = student.Email,
                Phone = student.Phone,
                Branch = student.Branch,
                Semester = student.Semester,
                EnrollmentYear = student.EnrollmentYear,
                Dob = student.Dob,
                Address = student.Address,
                City = student.City,
                Pincode = student.Pincode,
                CreatedAt = student.CreatedAt
            };

            return Ok(profile);
        }

        /// <summary>Update student profile</summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileDTO dto)
        {
            var student = await _context.Students.FindAsync(GetUserId())
                ?? throw new NotFoundException("Student not found");

            if (dto.Phone != null) student.Phone = dto.Phone;
            if (dto.Address != null) student.Address = dto.Address;
            if (dto.City != null) student.City = dto.City;
            if (dto.Pincode != null) student.Pincode = dto.Pincode;
            if (dto.Dob.HasValue) student.Dob = dto.Dob;
            if (dto.Semester.HasValue) student.Semester = dto.Semester.Value;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }

        /// <summary>Get student's enrollments</summary>
        [HttpGet("enrollments")]
        public async Task<IActionResult> GetEnrollments()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Subject)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Faculty)
                .Where(e => e.StudentId == GetUserId())
                .Select(e => new EnrollmentResponseDTO
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    RollNumber = e.Student.RollNumber,
                    SubjectId = e.SubjectId,
                    SubjectName = e.Subject.SubjectName,
                    CourseId = e.CourseId,
                    Semester = e.Semester,
                    AcademicYear = e.AcademicYear,
                    Status = e.Status,
                    EnrolledDate = e.EnrolledDate
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        /// <summary>Get student's attendance records</summary>
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendance([FromQuery] int? courseId)
        {
            var query = _context.Attendances
                .Include(a => a.Course)
                    .ThenInclude(c => c.Subject)
                .Include(a => a.MarkedByFaculty)
                .Where(a => a.StudentId == GetUserId());

            if (courseId.HasValue)
                query = query.Where(a => a.CourseId == courseId.Value);

            var attendance = await query
                .OrderByDescending(a => a.ClassDate)
                .Select(a => new AttendanceResponseDTO
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.Name,
                    CourseId = a.CourseId,
                    SubjectName = a.Course.Subject.SubjectName,
                    ClassDate = a.ClassDate,
                    Status = a.Status,
                    MarkedByName = a.MarkedByFaculty.Name
                })
                .ToListAsync();

            return Ok(attendance);
        }

        /// <summary>Get student's attendance summary per course</summary>
        [HttpGet("attendance/summary")]
        public async Task<IActionResult> GetAttendanceSummary()
        {
            var summary = await _context.Attendances
                .Include(a => a.Course)
                    .ThenInclude(c => c.Subject)
                .Where(a => a.StudentId == GetUserId())
                .GroupBy(a => new { a.CourseId, a.Course.Subject.SubjectName })
                .Select(g => new
                {
                    CourseId = g.Key.CourseId,
                    SubjectName = g.Key.SubjectName,
                    TotalClasses = g.Count(),
                    Present = g.Count(a => a.Status == "PRESENT"),
                    Absent = g.Count(a => a.Status == "ABSENT"),
                    Leave = g.Count(a => a.Status == "LEAVE"),
                    Percentage = Math.Round(g.Count(a => a.Status == "PRESENT") * 100.0 / g.Count(), 2)
                })
                .ToListAsync();

            return Ok(summary);
        }

        /// <summary>Get student's marks</summary>
        [HttpGet("marks")]
        public async Task<IActionResult> GetMarks([FromQuery] int? semester)
        {
            var query = _context.Marks
                .Include(m => m.Subject)
                .Where(m => m.StudentId == GetUserId());

            if (semester.HasValue)
                query = query.Where(m => m.Semester == semester.Value);

            var marks = await query
                .Select(m => new MarksResponseDTO
                {
                    Id = m.Id,
                    StudentId = m.StudentId,
                    StudentName = m.Student.Name,
                    RollNumber = m.Student.RollNumber,
                    SubjectId = m.SubjectId,
                    SubjectName = m.Subject.SubjectName,
                    SubjectCode = m.Subject.SubjectCode,
                    TheoryMarks = m.TheoryMarks,
                    PracticalMarks = m.PracticalMarks,
                    TotalMarks = m.TotalMarks,
                    Grade = m.Grade,
                    Semester = m.Semester,
                    AcademicYear = m.AcademicYear
                })
                .ToListAsync();

            return Ok(marks);
        }

        /// <summary>Get student's fee payments</summary>
        [HttpGet("fees")]
        public async Task<IActionResult> GetFees()
        {
            var fees = await _context.FeePayments
                .Include(fp => fp.FeeStructure)
                .Where(fp => fp.StudentId == GetUserId())
                .Select(fp => new FeePaymentResponseDTO
                {
                    Id = fp.Id,
                    StudentId = fp.StudentId,
                    StudentName = fp.Student.Name,
                    FeeStructureId = fp.FeeStructureId,
                    AmountPaid = fp.AmountPaid,
                    PaymentDate = fp.PaymentDate,
                    TransactionId = fp.TransactionId,
                    PaymentStatus = fp.PaymentStatus,
                    ReceiptNumber = fp.ReceiptNumber
                })
                .ToListAsync();

            return Ok(fees);
        }

        /// <summary>Get fee structure for student's branch</summary>
        [HttpGet("fee-structure")]
        public async Task<IActionResult> GetFeeStructure()
        {
            var student = await _context.Students.FindAsync(GetUserId())
                ?? throw new NotFoundException("Student not found");

            var feeStructures = await _context.FeeStructures
                .Where(fs => fs.Branch == student.Branch)
                .Select(fs => new FeeStructureResponseDTO
                {
                    Id = fs.Id,
                    Branch = fs.Branch,
                    Semester = fs.Semester,
                    TuitionFee = fs.TuitionFee,
                    HostelFee = fs.HostelFee,
                    LibraryFee = fs.LibraryFee,
                    LabFee = fs.LabFee,
                    TotalFee = fs.TotalFee,
                    DueDate = fs.DueDate
                })
                .ToListAsync();

            return Ok(feeStructures);
        }
    }
}
