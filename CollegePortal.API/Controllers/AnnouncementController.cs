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
    [Route("api/announcements")]
    public class AnnouncementController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public AnnouncementController(CollegePortalDbContext context)
        {
            _context = context;
        }

        /// <summary>Get all announcements (filtered by audience)</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string? audience)
        {
            var query = _context.Announcements
                .Include(a => a.Admin)
                .Where(a => a.ExpiresAt == null || a.ExpiresAt >= DateOnly.FromDateTime(DateTime.UtcNow));

            if (!string.IsNullOrEmpty(audience))
                query = query.Where(a => a.TargetAudience == audience || a.TargetAudience == "ALL");

            var announcements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementResponseDTO
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    CreatedBy = a.CreatedBy,
                    CreatedByName = a.Admin.Name,
                    TargetAudience = a.TargetAudience,
                    CreatedAt = a.CreatedAt,
                    ExpiresAt = a.ExpiresAt
                })
                .ToListAsync();

            return Ok(announcements);
        }

        /// <summary>Get an announcement by ID</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Admin)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException("Announcement not found");

            return Ok(new AnnouncementResponseDTO
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                CreatedBy = announcement.CreatedBy,
                CreatedByName = announcement.Admin.Name,
                TargetAudience = announcement.TargetAudience,
                CreatedAt = announcement.CreatedAt,
                ExpiresAt = announcement.ExpiresAt
            });
        }

        /// <summary>Create a new announcement (Admin only)</summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDTO dto)
        {
            var adminId = int.Parse(User.FindFirst("userId")!.Value);

            var announcement = new Announcement
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = adminId,
                TargetAudience = dto.TargetAudience,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var admin = await _context.Admins.FindAsync(adminId);

            return Created("", new AnnouncementResponseDTO
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                CreatedBy = announcement.CreatedBy,
                CreatedByName = admin?.Name ?? "Unknown",
                TargetAudience = announcement.TargetAudience,
                CreatedAt = announcement.CreatedAt,
                ExpiresAt = announcement.ExpiresAt
            });
        }

        /// <summary>Update an announcement (Admin only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateAnnouncementDTO dto)
        {
            var announcement = await _context.Announcements.FindAsync(id)
                ?? throw new NotFoundException("Announcement not found");

            announcement.Title = dto.Title;
            announcement.Description = dto.Description;
            announcement.TargetAudience = dto.TargetAudience;
            announcement.ExpiresAt = dto.ExpiresAt;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Announcement updated successfully" });
        }

        /// <summary>Delete an announcement (Admin only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id)
                ?? throw new NotFoundException("Announcement not found");

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Announcement deleted successfully" });
        }
    }
}
