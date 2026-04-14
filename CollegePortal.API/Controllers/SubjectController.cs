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
    [Route("api/subjects")]
    public class SubjectController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public SubjectController(CollegePortalDbContext context)
        {
            _context = context;
        }

        /// <summary>Get all subjects (optionally filter by branch/semester)</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string? branch, [FromQuery] int? semester)
        {
            var query = _context.Subjects.AsQueryable();

            if (!string.IsNullOrEmpty(branch))
                query = query.Where(s => s.Branch == branch);
            if (semester.HasValue)
                query = query.Where(s => s.Semester == semester.Value);

            var subjects = await query
                .Select(s => new SubjectResponseDTO
                {
                    Id = s.Id,
                    SubjectCode = s.SubjectCode,
                    SubjectName = s.SubjectName,
                    Branch = s.Branch,
                    Semester = s.Semester,
                    Credits = s.Credits,
                    TheoryMarks = s.TheoryMarks,
                    PracticalMarks = s.PracticalMarks
                })
                .ToListAsync();

            return Ok(subjects);
        }

        /// <summary>Get a subject by ID</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var subject = await _context.Subjects.FindAsync(id)
                ?? throw new NotFoundException("Subject not found");

            return Ok(new SubjectResponseDTO
            {
                Id = subject.Id,
                SubjectCode = subject.SubjectCode,
                SubjectName = subject.SubjectName,
                Branch = subject.Branch,
                Semester = subject.Semester,
                Credits = subject.Credits,
                TheoryMarks = subject.TheoryMarks,
                PracticalMarks = subject.PracticalMarks
            });
        }

        /// <summary>Create a new subject (Admin only)</summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateSubjectDTO dto)
        {
            if (await _context.Subjects.AnyAsync(s => s.SubjectCode == dto.SubjectCode))
                throw new Exceptions.ValidationException("Subject code already exists");

            var subject = new Subject
            {
                SubjectCode = dto.SubjectCode,
                SubjectName = dto.SubjectName,
                Branch = dto.Branch,
                Semester = dto.Semester,
                Credits = dto.Credits,
                TheoryMarks = dto.TheoryMarks,
                PracticalMarks = dto.PracticalMarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return Created("", new SubjectResponseDTO
            {
                Id = subject.Id,
                SubjectCode = subject.SubjectCode,
                SubjectName = subject.SubjectName,
                Branch = subject.Branch,
                Semester = subject.Semester,
                Credits = subject.Credits,
                TheoryMarks = subject.TheoryMarks,
                PracticalMarks = subject.PracticalMarks
            });
        }

        /// <summary>Update a subject (Admin only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateSubjectDTO dto)
        {
            var subject = await _context.Subjects.FindAsync(id)
                ?? throw new NotFoundException("Subject not found");

            subject.SubjectCode = dto.SubjectCode;
            subject.SubjectName = dto.SubjectName;
            subject.Branch = dto.Branch;
            subject.Semester = dto.Semester;
            subject.Credits = dto.Credits;
            subject.TheoryMarks = dto.TheoryMarks;
            subject.PracticalMarks = dto.PracticalMarks;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subject updated successfully" });
        }

        /// <summary>Delete a subject (Admin only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _context.Subjects.FindAsync(id)
                ?? throw new NotFoundException("Subject not found");

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Subject deleted successfully" });
        }
    }
}
