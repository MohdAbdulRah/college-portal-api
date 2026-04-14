namespace CollegePortal.API.DTOs.Profile
{
    public class StudentProfileDTO
    {
        public int Id { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public int Semester { get; set; }
        public int EnrollmentYear { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateStudentProfileDTO
    {
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public DateOnly? Dob { get; set; }
        public int? Semester { get; set; }
    }

    public class FacultyProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public int ExperienceYears { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateFacultyProfileDTO
    {
        public string? Phone { get; set; }
        public string? Qualification { get; set; }
        public int? ExperienceYears { get; set; }
    }
}
