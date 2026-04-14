using System.ComponentModel.DataAnnotations;

namespace CollegePortal.API.DTOs.Entity
{
    // ── Subject ──
    public class CreateSubjectDTO
    {
        [Required, MaxLength(20)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Branch { get; set; } = string.Empty;

        [Range(1, 8)]
        public int Semester { get; set; }

        public int Credits { get; set; }
        public int TheoryMarks { get; set; } = 100;
        public int PracticalMarks { get; set; } = 50;
    }

    public class SubjectResponseDTO
    {
        public int Id { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public int Semester { get; set; }
        public int Credits { get; set; }
        public int TheoryMarks { get; set; }
        public int PracticalMarks { get; set; }
    }

    // ── Course ──
    public class CreateCourseDTO
    {
        [Required]
        public int FacultyId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Range(1, 8)]
        public int Semester { get; set; }

        [MaxLength(10)]
        public string Section { get; set; } = string.Empty;

        [MaxLength(10)]
        public string AcademicYear { get; set; } = string.Empty;

        public int TotalClasses { get; set; }
    }

    public class CourseResponseDTO
    {
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int Semester { get; set; }
        public string Section { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
    }

    // ── Enrollment ──
    public class CreateEnrollmentDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Range(1, 8)]
        public int Semester { get; set; }

        [MaxLength(10)]
        public string AcademicYear { get; set; } = string.Empty;
    }

    public class EnrollmentResponseDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int Semester { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolledDate { get; set; }
    }

    // ── Attendance ──
    public class MarkAttendanceDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateOnly ClassDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = string.Empty; // PRESENT, ABSENT, LEAVE
    }

    public class BulkAttendanceDTO
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateOnly ClassDate { get; set; }

        [Required]
        public List<AttendanceEntryDTO> Entries { get; set; } = new();
    }

    public class AttendanceEntryDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }

    public class AttendanceResponseDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public DateOnly ClassDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MarkedByName { get; set; } = string.Empty;
    }

    // ── Marks ──
    public class CreateMarksDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int TheoryMarks { get; set; }
        public int PracticalMarks { get; set; }

        [Range(1, 8)]
        public int Semester { get; set; }

        [MaxLength(10)]
        public string AcademicYear { get; set; } = string.Empty;
    }

    public class MarksResponseDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int TheoryMarks { get; set; }
        public int PracticalMarks { get; set; }
        public int TotalMarks { get; set; }
        public string? Grade { get; set; }
        public int Semester { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
    }

    // ── Fee Structure ──
    public class CreateFeeStructureDTO
    {
        [Required, MaxLength(50)]
        public string Branch { get; set; } = string.Empty;

        [Range(1, 8)]
        public int Semester { get; set; }

        public decimal TuitionFee { get; set; }
        public decimal HostelFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal LabFee { get; set; }
        public DateOnly DueDate { get; set; }
    }

    public class FeeStructureResponseDTO
    {
        public int Id { get; set; }
        public string Branch { get; set; } = string.Empty;
        public int Semester { get; set; }
        public decimal TuitionFee { get; set; }
        public decimal HostelFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal LabFee { get; set; }
        public decimal TotalFee { get; set; }
        public DateOnly DueDate { get; set; }
    }

    // ── Fee Payment ──
    public class CreateFeePaymentDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int FeeStructureId { get; set; }

        public decimal AmountPaid { get; set; }

        [MaxLength(100)]
        public string? TransactionId { get; set; }
    }

    public class FeePaymentResponseDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int FeeStructureId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? ReceiptNumber { get; set; }
    }

    // ── Announcement ──
    public class CreateAnnouncementDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required, MaxLength(50)]
        public string TargetAudience { get; set; } = "ALL"; // STUDENT, FACULTY, ALL

        public DateOnly? ExpiresAt { get; set; }
    }

    public class AnnouncementResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateOnly? ExpiresAt { get; set; }
    }

    // ── Dashboard / Reports ──
    public class DashboardStatsDTO
    {
        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveEnrollments { get; set; }
        public decimal TotalFeesCollected { get; set; }
        public int PendingPayments { get; set; }
    }
}
