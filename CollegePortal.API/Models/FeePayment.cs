using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePortal.API.Models
{
    [Table("fee_payments")]
    public class FeePayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [Required]
        public int FeeStructureId { get; set; }

        [ForeignKey("FeeStructureId")]
        public FeeStructure FeeStructure { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [Required, MaxLength(20)]
        public string PaymentStatus { get; set; } = "PENDING"; // PENDING, COMPLETED, FAILED

        [MaxLength(50)]
        public string? ReceiptNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
