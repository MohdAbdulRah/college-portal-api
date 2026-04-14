using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePortal.API.Models
{
    [Table("faculty_personal")]
    public class FacultyPersonal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Designation { get; set; } = string.Empty; // Professor, Associate Prof, Assistant Prof

        [Required, MaxLength(50)]
        public string Department { get; set; } = string.Empty; // CSE, ECE, ME, CE, EE

        [MaxLength(100)]
        public string? Qualification { get; set; }

        public int ExperienceYears { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Attendance> MarkedAttendances { get; set; } = new List<Attendance>();
    }
}
