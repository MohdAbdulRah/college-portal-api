using CollegePortal.API.Data;
using CollegePortal.API.DTOs.Entity;
using CollegePortal.API.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegePortal.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public AdminController(CollegePortalDbContext context)
        {
            _context = context;
        }

        /// <summary>Get dashboard statistics</summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var stats = new DashboardStatsDTO
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalFaculty = await _context.Faculty.CountAsync(),
                TotalSubjects = await _context.Subjects.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                ActiveEnrollments = await _context.Enrollments.CountAsync(e => e.Status == "ACTIVE"),
                TotalFeesCollected = await _context.FeePayments
                    .Where(fp => fp.PaymentStatus == "COMPLETED")
                    .SumAsync(fp => fp.AmountPaid),
                PendingPayments = await _context.FeePayments.CountAsync(fp => fp.PaymentStatus == "PENDING")
            };

            return Ok(stats);
        }

        /// <summary>Get all students with optional filters</summary>
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents([FromQuery] string? branch, [FromQuery] int? semester)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(branch))
                query = query.Where(s => s.Branch == branch);
            if (semester.HasValue)
                query = query.Where(s => s.Semester == semester.Value);

            var students = await query
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id, s.RollNumber, s.Name, s.Email, s.Phone,
                    s.Branch, s.Semester, s.EnrollmentYear, s.CreatedAt
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>Get a specific student by ID</summary>
        [HttpGet("students/{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id)
                ?? throw new NotFoundException("Student not found");

            return Ok(new
            {
                student.Id, student.RollNumber, student.Name, student.Email,
                student.Phone, student.Branch, student.Semester, student.EnrollmentYear,
                student.Dob, student.Address, student.City, student.Pincode, student.CreatedAt
            });
        }

        /// <summary>Delete a student</summary>
        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id)
                ?? throw new NotFoundException("Student not found");

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Student deleted successfully" });
        }

        /// <summary>Get all faculty with optional filters</summary>
        [HttpGet("faculty")]
        public async Task<IActionResult> GetAllFaculty([FromQuery] string? department)
        {
            var query = _context.Faculty.AsQueryable();

            if (!string.IsNullOrEmpty(department))
                query = query.Where(f => f.Department == department);

            var faculty = await query
                .OrderBy(f => f.Name)
                .Select(f => new
                {
                    f.Id, f.Name, f.Email, f.Phone,
                    f.Designation, f.Department, f.Qualification,
                    f.ExperienceYears, f.CreatedAt
                })
                .ToListAsync();

            return Ok(faculty);
        }

        /// <summary>Delete a faculty member</summary>
        [HttpDelete("faculty/{id}")]
        public async Task<IActionResult> DeleteFaculty(int id)
        {
            var faculty = await _context.Faculty.FindAsync(id)
                ?? throw new NotFoundException("Faculty not found");

            _context.Faculty.Remove(faculty);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Faculty deleted successfully" });
        }

        /// <summary>Get all admins</summary>
        [HttpGet("admins")]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _context.Admins
                .Select(a => new { a.Id, a.Name, a.Email, a.Role, a.Phone, a.CreatedAt })
                .ToListAsync();

            return Ok(admins);
        }

        /// <summary>Get reports: enrollment summary by branch</summary>
        [HttpGet("reports/enrollment")]
        public async Task<IActionResult> GetEnrollmentReport()
        {
            var report = await _context.Enrollments
                .Include(e => e.Student)
                .GroupBy(e => new { e.Student.Branch, e.Semester, e.AcademicYear })
                .Select(g => new
                {
                    Branch = g.Key.Branch,
                    Semester = g.Key.Semester,
                    AcademicYear = g.Key.AcademicYear,
                    TotalEnrollments = g.Count(),
                    Active = g.Count(e => e.Status == "ACTIVE"),
                    Dropped = g.Count(e => e.Status == "DROPPED"),
                    Completed = g.Count(e => e.Status == "COMPLETED")
                })
                .ToListAsync();

            return Ok(report);
        }

        /// <summary>Get reports: fee collection summary</summary>
        [HttpGet("reports/fees")]
        public async Task<IActionResult> GetFeeReport()
        {
            var report = await _context.FeePayments
                .Include(fp => fp.Student)
                .GroupBy(fp => new { fp.Student.Branch, fp.PaymentStatus })
                .Select(g => new
                {
                    Branch = g.Key.Branch,
                    Status = g.Key.PaymentStatus,
                    Count = g.Count(),
                    TotalAmount = g.Sum(fp => fp.AmountPaid)
                })
                .ToListAsync();

            return Ok(report);
        }

        /// <summary>Get reports: attendance summary by course</summary>
        [HttpGet("reports/attendance")]
        public async Task<IActionResult> GetAttendanceReport()
        {
            var report = await _context.Attendances
                .Include(a => a.Course)
                    .ThenInclude(c => c.Subject)
                .GroupBy(a => new { a.CourseId, a.Course.Subject.SubjectName })
                .Select(g => new
                {
                    CourseId = g.Key.CourseId,
                    SubjectName = g.Key.SubjectName,
                    TotalRecords = g.Count(),
                    Present = g.Count(a => a.Status == "PRESENT"),
                    Absent = g.Count(a => a.Status == "ABSENT"),
                    Leave = g.Count(a => a.Status == "LEAVE"),
                    AttendanceRate = Math.Round(g.Count(a => a.Status == "PRESENT") * 100.0 / g.Count(), 2)
                })
                .ToListAsync();

            return Ok(report);
        }
    }
}
