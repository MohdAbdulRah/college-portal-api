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
    [Route("api/courses")]
    public class CourseController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public CourseController(CollegePortalDbContext context)
        {
            _context = context;
        }

        /// <summary>Get all courses (optionally filter by semester/section)</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int? semester, [FromQuery] string? section)
        {
            var query = _context.Courses
                .Include(c => c.Faculty)
                .Include(c => c.Subject)
                .AsQueryable();

            if (semester.HasValue)
                query = query.Where(c => c.Semester == semester.Value);
            if (!string.IsNullOrEmpty(section))
                query = query.Where(c => c.Section == section);

            var courses = await query
                .Select(c => new CourseResponseDTO
                {
                    Id = c.Id,
                    FacultyId = c.FacultyId,
                    FacultyName = c.Faculty.Name,
                    SubjectId = c.SubjectId,
                    SubjectName = c.Subject.SubjectName,
                    SubjectCode = c.Subject.SubjectCode,
                    Semester = c.Semester,
                    Section = c.Section,
                    AcademicYear = c.AcademicYear,
                    TotalClasses = c.TotalClasses
                })
                .ToListAsync();

            return Ok(courses);
        }

        /// <summary>Get a course by ID</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Faculty)
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Course not found");

            return Ok(new CourseResponseDTO
            {
                Id = course.Id,
                FacultyId = course.FacultyId,
                FacultyName = course.Faculty.Name,
                SubjectId = course.SubjectId,
                SubjectName = course.Subject.SubjectName,
                SubjectCode = course.Subject.SubjectCode,
                Semester = course.Semester,
                Section = course.Section,
                AcademicYear = course.AcademicYear,
                TotalClasses = course.TotalClasses
            });
        }

        /// <summary>Create a new course (Admin only)</summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateCourseDTO dto)
        {
            if (!await _context.Faculty.AnyAsync(f => f.Id == dto.FacultyId))
                throw new NotFoundException("Faculty not found");
            if (!await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                throw new NotFoundException("Subject not found");

            var course = new Course
            {
                FacultyId = dto.FacultyId,
                SubjectId = dto.SubjectId,
                Semester = dto.Semester,
                Section = dto.Section,
                AcademicYear = dto.AcademicYear,
                TotalClasses = dto.TotalClasses,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var created = await _context.Courses
                .Include(c => c.Faculty)
                .Include(c => c.Subject)
                .FirstAsync(c => c.Id == course.Id);

            return Created("", new CourseResponseDTO
            {
                Id = created.Id,
                FacultyId = created.FacultyId,
                FacultyName = created.Faculty.Name,
                SubjectId = created.SubjectId,
                SubjectName = created.Subject.SubjectName,
                SubjectCode = created.Subject.SubjectCode,
                Semester = created.Semester,
                Section = created.Section,
                AcademicYear = created.AcademicYear,
                TotalClasses = created.TotalClasses
            });
        }

        /// <summary>Update a course (Admin only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCourseDTO dto)
        {
            var course = await _context.Courses.FindAsync(id)
                ?? throw new NotFoundException("Course not found");

            course.FacultyId = dto.FacultyId;
            course.SubjectId = dto.SubjectId;
            course.Semester = dto.Semester;
            course.Section = dto.Section;
            course.AcademicYear = dto.AcademicYear;
            course.TotalClasses = dto.TotalClasses;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Course updated successfully" });
        }

        /// <summary>Delete a course (Admin only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id)
                ?? throw new NotFoundException("Course not found");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Course deleted successfully" });
        }
    }
}
