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
    [Route("api/fees")]
    public class FeeController : ControllerBase
    {
        private readonly CollegePortalDbContext _context;

        public FeeController(CollegePortalDbContext context)
        {
            _context = context;
        }

        // ═══════════════ Fee Structure ═══════════════

        /// <summary>Get all fee structures (optionally filter by branch)</summary>
        [HttpGet("structure")]
        [Authorize]
        public async Task<IActionResult> GetAllStructures([FromQuery] string? branch, [FromQuery] int? semester)
        {
            var query = _context.FeeStructures.AsQueryable();

            if (!string.IsNullOrEmpty(branch)) query = query.Where(f => f.Branch == branch);
            if (semester.HasValue) query = query.Where(f => f.Semester == semester.Value);

            var structures = await query
                .Select(fs => new FeeStructureResponseDTO
                {
                    Id = fs.Id,
                    Branch = fs.Branch,
                    Semester = fs.Semester,
                    TuitionFee = fs.TuitionFee,
                    HostelFee = fs.HostelFee,
                    LibraryFee = fs.LibraryFee,
                    LabFee = fs.LabFee,
                    TotalFee = fs.TotalFee,
                    DueDate = fs.DueDate
                })
                .ToListAsync();

            return Ok(structures);
        }

        /// <summary>Create a fee structure (Admin only)</summary>
        [HttpPost("structure")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateStructure([FromBody] CreateFeeStructureDTO dto)
        {
            var feeStructure = new FeeStructure
            {
                Branch = dto.Branch,
                Semester = dto.Semester,
                TuitionFee = dto.TuitionFee,
                HostelFee = dto.HostelFee,
                LibraryFee = dto.LibraryFee,
                LabFee = dto.LabFee,
                TotalFee = dto.TuitionFee + dto.HostelFee + dto.LibraryFee + dto.LabFee,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.FeeStructures.Add(feeStructure);
            await _context.SaveChangesAsync();

            return Created("", new FeeStructureResponseDTO
            {
                Id = feeStructure.Id,
                Branch = feeStructure.Branch,
                Semester = feeStructure.Semester,
                TuitionFee = feeStructure.TuitionFee,
                HostelFee = feeStructure.HostelFee,
                LibraryFee = feeStructure.LibraryFee,
                LabFee = feeStructure.LabFee,
                TotalFee = feeStructure.TotalFee,
                DueDate = feeStructure.DueDate
            });
        }

        /// <summary>Update a fee structure (Admin only)</summary>
        [HttpPut("structure/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStructure(int id, [FromBody] CreateFeeStructureDTO dto)
        {
            var feeStructure = await _context.FeeStructures.FindAsync(id)
                ?? throw new NotFoundException("Fee structure not found");

            feeStructure.Branch = dto.Branch;
            feeStructure.Semester = dto.Semester;
            feeStructure.TuitionFee = dto.TuitionFee;
            feeStructure.HostelFee = dto.HostelFee;
            feeStructure.LibraryFee = dto.LibraryFee;
            feeStructure.LabFee = dto.LabFee;
            feeStructure.TotalFee = dto.TuitionFee + dto.HostelFee + dto.LibraryFee + dto.LabFee;
            feeStructure.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Fee structure updated successfully" });
        }

        /// <summary>Delete a fee structure (Admin only)</summary>
        [HttpDelete("structure/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStructure(int id)
        {
            var feeStructure = await _context.FeeStructures.FindAsync(id)
                ?? throw new NotFoundException("Fee structure not found");

            _context.FeeStructures.Remove(feeStructure);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Fee structure deleted successfully" });
        }

        // ═══════════════ Fee Payments ═══════════════

        /// <summary>Get all payments (Admin only, with filters)</summary>
        [HttpGet("payments")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllPayments([FromQuery] int? studentId,
            [FromQuery] string? status)
        {
            var query = _context.FeePayments
                .Include(fp => fp.Student)
                .AsQueryable();

            if (studentId.HasValue) query = query.Where(fp => fp.StudentId == studentId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(fp => fp.PaymentStatus == status);

            var payments = await query
                .OrderByDescending(fp => fp.PaymentDate)
                .Select(fp => new FeePaymentResponseDTO
                {
                    Id = fp.Id,
                    StudentId = fp.StudentId,
                    StudentName = fp.Student.Name,
                    FeeStructureId = fp.FeeStructureId,
                    AmountPaid = fp.AmountPaid,
                    PaymentDate = fp.PaymentDate,
                    TransactionId = fp.TransactionId,
                    PaymentStatus = fp.PaymentStatus,
                    ReceiptNumber = fp.ReceiptNumber
                })
                .ToListAsync();

            return Ok(payments);
        }

        /// <summary>Make a fee payment</summary>
        [HttpPost("payments")]
        [Authorize(Roles = "STUDENT,ADMIN")]
        public async Task<IActionResult> MakePayment([FromBody] CreateFeePaymentDTO dto)
        {
            if (!await _context.Students.AnyAsync(s => s.Id == dto.StudentId))
                throw new NotFoundException("Student not found");
            if (!await _context.FeeStructures.AnyAsync(fs => fs.Id == dto.FeeStructureId))
                throw new NotFoundException("Fee structure not found");

            var payment = new FeePayment
            {
                StudentId = dto.StudentId,
                FeeStructureId = dto.FeeStructureId,
                AmountPaid = dto.AmountPaid,
                PaymentDate = DateTime.UtcNow,
                TransactionId = dto.TransactionId ?? Guid.NewGuid().ToString("N")[..16].ToUpper(),
                PaymentStatus = "COMPLETED",
                ReceiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                CreatedAt = DateTime.UtcNow
            };

            _context.FeePayments.Add(payment);
            await _context.SaveChangesAsync();

            return Created("", new FeePaymentResponseDTO
            {
                Id = payment.Id,
                StudentId = payment.StudentId,
                FeeStructureId = payment.FeeStructureId,
                AmountPaid = payment.AmountPaid,
                PaymentDate = payment.PaymentDate,
                TransactionId = payment.TransactionId,
                PaymentStatus = payment.PaymentStatus,
                ReceiptNumber = payment.ReceiptNumber
            });
        }

        /// <summary>Update payment status (Admin only)</summary>
        [HttpPut("payments/{id}/status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDTO dto)
        {
            var payment = await _context.FeePayments.FindAsync(id)
                ?? throw new NotFoundException("Payment not found");

            payment.PaymentStatus = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Payment status updated successfully" });
        }
    }

    public class UpdatePaymentStatusDTO
    {
        public string Status { get; set; } = string.Empty; // PENDING, COMPLETED, FAILED
    }
}
