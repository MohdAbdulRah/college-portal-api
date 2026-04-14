using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePortal.API.Models
{
    [Table("enrollments")]
    public class Enrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [Required]
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;

        [Range(1, 8)]
        public int Semester { get; set; }

        [MaxLength(10)]
        public string AcademicYear { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, DROPPED, COMPLETED

        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;
    }
}
