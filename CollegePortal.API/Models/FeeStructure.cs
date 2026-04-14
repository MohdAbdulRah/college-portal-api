using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePortal.API.Models
{
    [Table("fee_structure")]
    public class FeeStructure
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Branch { get; set; } = string.Empty;

        [Range(1, 8)]
        public int Semester { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TuitionFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HostelFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LibraryFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LabFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalFee { get; set; }

        public DateOnly DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();
    }
}
