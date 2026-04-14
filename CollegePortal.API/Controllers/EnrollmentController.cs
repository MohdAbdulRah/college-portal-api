using CollegePortal.API.Data;
using CollegePortal.API.DTOs.Entity;
using CollegePortal.API.Exceptions;
using CollegePortal.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegePortal.API.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize(Roles = "ADMIN")]
    public class EnrollmentController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public EnrollmentController(CollegePortalDbContext context)
        {
            _context = context;
        }

        /// <summary>Get all enrollments with optional filters</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? studentId, [FromQuery] int? courseId,
            [FromQuery] string? status)
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .Include(e => e.Course)
                .AsQueryable();

            if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);
            if (courseId.HasValue) query = query.Where(e => e.CourseId == courseId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(e => e.Status == status);

            var enrollments = await query
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

        /// <summary>Create a new enrollment</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentDTO dto)
        {
            if (!await _context.Students.AnyAsync(s => s.Id == dto.StudentId))
                throw new NotFoundException("Student not found");
            if (!await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                throw new NotFoundException("Subject not found");
            if (!await _context.Courses.AnyAsync(c => c.Id == dto.CourseId))
                throw new NotFoundException("Course not found");

            // Check for duplicate enrollment
            var exists = await _context.Enrollments
                .AnyAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId
                    && e.Status == "ACTIVE");
            if (exists)
                throw new Exceptions.ValidationException("Student is already enrolled in this course");

            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                SubjectId = dto.SubjectId,
                CourseId = dto.CourseId,
                Semester = dto.Semester,
                AcademicYear = dto.AcademicYear,
                Status = "ACTIVE",
                EnrolledDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Created("", new { enrollment.Id, message = "Enrollment created successfully" });
        }

        /// <summary>Update enrollment status</summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrollmentStatusDTO dto)
        {
            var enrollment = await _context.Enrollments.FindAsync(id)
                ?? throw new NotFoundException("Enrollment not found");

            enrollment.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Enrollment status updated successfully" });
        }

        /// <summary>Delete an enrollment</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id)
                ?? throw new NotFoundException("Enrollment not found");

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Enrollment deleted successfully" });
        }
    }

    public class UpdateEnrollmentStatusDTO
    {
        public string Status { get; set; } = string.Empty; // ACTIVE, DROPPED, COMPLETED
    }
}
