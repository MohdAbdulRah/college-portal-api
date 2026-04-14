using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePortal.API.Models
{
    [Table("subjects")]
    public class Subject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string SubjectCode { get; set; } = string.Empty; // CS101, CS102

        [Required, MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Branch { get; set; } = string.Empty;

        [Range(1, 8)]
        public int Semester { get; set; }

        public int Credits { get; set; }

        public int TheoryMarks { get; set; } = 100;

        public int PracticalMarks { get; set; } = 50;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Marks> Marks { get; set; } = new List<Marks>();
    }
}
