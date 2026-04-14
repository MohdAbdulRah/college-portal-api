using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePortal.API.Models
{
    [Table("announcements")]
    public class Announcement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public Admin Admin { get; set; } = null!;

        [Required, MaxLength(50)]
        public string TargetAudience { get; set; } = "ALL"; // STUDENT, FACULTY, ALL

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateOnly? ExpiresAt { get; set; }
    }
}
