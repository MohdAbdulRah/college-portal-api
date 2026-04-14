using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePortal.API.Models
{
    [Table("attendance")]
    public class Attendance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;

        [Required]
        public DateOnly ClassDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = string.Empty; // PRESENT, ABSENT, LEAVE

        [Required]
        public int MarkedBy { get; set; }

        [ForeignKey("MarkedBy")]
        public FacultyPersonal MarkedByFaculty { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
