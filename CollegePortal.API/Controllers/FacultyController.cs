using CollegePortal.API.Data;
using CollegePortal.API.DTOs.Profile;
using CollegePortal.API.DTOs.Entity;
using CollegePortal.API.Exceptions;
using CollegePortal.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegePortal.API.Controllers
{
    [ApiController]
    [Route("api/faculty")]
    [Authorize(Roles = "FACULTY")]
    public class FacultyController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public FacultyController(CollegePortalDbContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

        /// <summary>Get faculty profile</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var faculty = await _context.Faculty.FindAsync(GetUserId())
                ?? throw new NotFoundException("Faculty not found");

            var profile = new FacultyProfileDTO
            {
                Id = faculty.Id,
                Name = faculty.Name,
                Email = faculty.Email,
                Phone = faculty.Phone,
                Designation = faculty.Designation,
                Department = faculty.Department,
                Qualification = faculty.Qualification,
                ExperienceYears = faculty.ExperienceYears,
                CreatedAt = faculty.CreatedAt
            };

            return Ok(profile);
        }

        /// <summary>Update faculty profile</summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateFacultyProfileDTO dto)
        {
            var faculty = await _context.Faculty.FindAsync(GetUserId())
                ?? throw new NotFoundException("Faculty not found");

            if (dto.Phone != null) faculty.Phone = dto.Phone;
            if (dto.Qualification != null) faculty.Qualification = dto.Qualification;
            if (dto.ExperienceYears.HasValue) faculty.ExperienceYears = dto.ExperienceYears.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }

        /// <summary>Get courses assigned to this faculty</summary>
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => c.FacultyId == GetUserId())
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

        /// <summary>Get students enrolled in a course taught by this faculty</summary>
        [HttpGet("courses/{courseId}/students")]
        public async Task<IActionResult> GetStudentsInCourse(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId)
                ?? throw new NotFoundException("Course not found");

            if (course.FacultyId != GetUserId())
                throw new ForbiddenException("You are not assigned to this course");

            var students = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId && e.Status == "ACTIVE")
                .Select(e => new
                {
                    e.Student.Id,
                    e.Student.RollNumber,
                    e.Student.Name,
                    e.Student.Email,
                    e.Student.Branch,
                    e.Student.Semester,
                    EnrollmentStatus = e.Status
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>Mark attendance for a course (single student)</summary>
        [HttpPost("attendance")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDTO dto)
        {
            var course = await _context.Courses.FindAsync(dto.CourseId)
                ?? throw new NotFoundException("Course not found");

            if (course.FacultyId != GetUserId())
                throw new ForbiddenException("You are not assigned to this course");

            // Check if attendance already exists
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == dto.StudentId &&
                    a.CourseId == dto.CourseId && a.ClassDate == dto.ClassDate);

            if (existing != null)
            {
                existing.Status = dto.Status;
                existing.MarkedBy = GetUserId();
            }
            else
            {
                var attendance = new Attendance
                {
                    StudentId = dto.StudentId,
                    CourseId = dto.CourseId,
                    ClassDate = dto.ClassDate,
                    Status = dto.Status,
                    MarkedBy = GetUserId(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Attendances.Add(attendance);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Attendance marked successfully" });
        }

        /// <summary>Mark bulk attendance for a course</summary>
        [HttpPost("attendance/bulk")]
        public async Task<IActionResult> MarkBulkAttendance([FromBody] BulkAttendanceDTO dto)
        {
            var course = await _context.Courses.FindAsync(dto.CourseId)
                ?? throw new NotFoundException("Course not found");

            if (course.FacultyId != GetUserId())
                throw new ForbiddenException("You are not assigned to this course");

            foreach (var entry in dto.Entries)
            {
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.StudentId == entry.StudentId &&
                        a.CourseId == dto.CourseId && a.ClassDate == dto.ClassDate);

                if (existing != null)
                {
                    existing.Status = entry.Status;
                    existing.MarkedBy = GetUserId();
                }
                else
                {
                    _context.Attendances.Add(new Attendance
                    {
                        StudentId = entry.StudentId,
                        CourseId = dto.CourseId,
                        ClassDate = dto.ClassDate,
                        Status = entry.Status,
                        MarkedBy = GetUserId(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Attendance marked for {dto.Entries.Count} students" });
        }

        /// <summary>Get attendance records for a course</summary>
        [HttpGet("courses/{courseId}/attendance")]
        public async Task<IActionResult> GetCourseAttendance(int courseId, [FromQuery] DateOnly? date)
        {
            var course = await _context.Courses.FindAsync(courseId)
                ?? throw new NotFoundException("Course not found");

            if (course.FacultyId != GetUserId())
                throw new ForbiddenException("You are not assigned to this course");

            var query = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                    .ThenInclude(c => c.Subject)
                .Where(a => a.CourseId == courseId);

            if (date.HasValue)
                query = query.Where(a => a.ClassDate == date.Value);

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

        /// <summary>Enter marks for a student</summary>
        [HttpPost("marks")]
        public async Task<IActionResult> EnterMarks([FromBody] CreateMarksDTO dto)
        {
            var course = await _context.Courses.FindAsync(dto.CourseId)
                ?? throw new NotFoundException("Course not found");

            if (course.FacultyId != GetUserId())
                throw new ForbiddenException("You are not assigned to this course");

            var totalMarks = dto.TheoryMarks + dto.PracticalMarks;
            var grade = CalculateGrade(totalMarks, 150); // default max 100+50

            var existing = await _context.Marks
                .FirstOrDefaultAsync(m => m.StudentId == dto.StudentId &&
                    m.CourseId == dto.CourseId);

            if (existing != null)
            {
                existing.TheoryMarks = dto.TheoryMarks;
                existing.PracticalMarks = dto.PracticalMarks;
                existing.TotalMarks = totalMarks;
                existing.Grade = grade;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var marks = new Marks
                {
                    StudentId = dto.StudentId,
                    SubjectId = dto.SubjectId,
                    CourseId = dto.CourseId,
                    TheoryMarks = dto.TheoryMarks,
                    PracticalMarks = dto.PracticalMarks,
                    TotalMarks = totalMarks,
                    Grade = grade,
                    Semester = dto.Semester,
                    AcademicYear = dto.AcademicYear,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Marks.Add(marks);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Marks entered successfully", totalMarks, grade });
        }

        /// <summary>Get marks for a course</summary>
        [HttpGet("courses/{courseId}/marks")]
        public async Task<IActionResult> GetCourseMarks(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId)
                ?? throw new NotFoundException("Course not found");

            if (course.FacultyId != GetUserId())
                throw new ForbiddenException("You are not assigned to this course");

            var marks = await _context.Marks
                .Include(m => m.Student)
                .Include(m => m.Subject)
                .Where(m => m.CourseId == courseId)
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

        private static string CalculateGrade(int totalMarks, int maxMarks)
        {
            var percentage = (double)totalMarks / maxMarks * 100;
            return percentage switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B+",
                >= 60 => "B",
                >= 50 => "C",
                >= 40 => "D",
                _ => "F"
            };
        }
    }
}
