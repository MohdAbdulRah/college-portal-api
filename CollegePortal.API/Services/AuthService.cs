using CollegePortal.API.Data;
using CollegePortal.API.DTOs.Auth;
using CollegePortal.API.Exceptions;
using CollegePortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegePortal.API.Services
{
    public interface IAuthService
    {
        Task<RegisterResponseDTO> RegisterStudentAsync(StudentRegisterDTO dto);
        Task<RegisterResponseDTO> RegisterFacultyAsync(FacultyRegisterDTO dto);
        Task<RegisterResponseDTO> RegisterAdminAsync(AdminRegisterDTO dto);
        Task<JwtResponseDTO> LoginStudentAsync(LoginDTO dto);
        Task<JwtResponseDTO> LoginFacultyAsync(LoginDTO dto);
        Task<JwtResponseDTO> LoginAdminAsync(LoginDTO dto);
        Task<string> ForgotPasswordAsync(ForgotPasswordDTO dto);
        Task<string> ResetPasswordAsync(ResetPasswordDTO dto);
        Task<string> ChangePasswordAsync(int userId, string role, ChangePasswordDTO dto);
    }

    public class AuthService : IAuthService
    {
        private readonly CollegePortalDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(CollegePortalDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // ── Student Registration ──
        public async Task<RegisterResponseDTO> RegisterStudentAsync(StudentRegisterDTO dto)
        {
            if (await _context.Students.AnyAsync(s => s.Email == dto.Email))
                throw new UserAlreadyExistsException("Student with this email already exists");

            ValidatePassword(dto.Password);

            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Branch = dto.Branch,
                Semester = 1,
                EnrollmentYear = dto.EnrollmentYear,
                RollNumber = GenerateRollNumber(dto.Branch, dto.EnrollmentYear),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return new RegisterResponseDTO
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Role = "STUDENT",
                Message = "Registration successful. Please login."
            };
        }

        // ── Faculty Registration ──
        public async Task<RegisterResponseDTO> RegisterFacultyAsync(FacultyRegisterDTO dto)
        {
            if (await _context.Faculty.AnyAsync(f => f.Email == dto.Email))
                throw new UserAlreadyExistsException("Faculty with this email already exists");

            ValidatePassword(dto.Password);

            var faculty = new FacultyPersonal
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Department = dto.Department,
                Designation = dto.Designation,
                Qualification = dto.Qualification,
                ExperienceYears = dto.ExperienceYears,
                CreatedAt = DateTime.UtcNow
            };

            _context.Faculty.Add(faculty);
            await _context.SaveChangesAsync();

            return new RegisterResponseDTO
            {
                Id = faculty.Id,
                Name = faculty.Name,
                Email = faculty.Email,
                Role = "FACULTY",
                Message = "Registration successful. Please login."
            };
        }

        // ── Admin Registration ──
        public async Task<RegisterResponseDTO> RegisterAdminAsync(AdminRegisterDTO dto)
        {
            if (await _context.Admins.AnyAsync(a => a.Email == dto.Email))
                throw new UserAlreadyExistsException("Admin with this email already exists");

            ValidatePassword(dto.Password);

            var admin = new Admin
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                Phone = dto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return new RegisterResponseDTO
            {
                Id = admin.Id,
                Name = admin.Name,
                Email = admin.Email,
                Role = admin.Role,
                Message = "Registration successful. Please login."
            };
        }

        // ── Student Login ──
        public async Task<JwtResponseDTO> LoginStudentAsync(LoginDTO dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == dto.Email)
                ?? throw new InvalidCredentialsException();

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash))
                throw new InvalidCredentialsException();

            var token = _jwtService.GenerateToken(student.Id, student.Email, student.Name, "STUDENT");

            return new JwtResponseDTO
            {
                Token = token,
                Type = "Bearer",
                UserId = student.Id,
                Email = student.Email,
                Name = student.Name,
                Role = "STUDENT"
            };
        }

        // ── Faculty Login ──
        public async Task<JwtResponseDTO> LoginFacultyAsync(LoginDTO dto)
        {
            var faculty = await _context.Faculty.FirstOrDefaultAsync(f => f.Email == dto.Email)
                ?? throw new InvalidCredentialsException();

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, faculty.PasswordHash))
                throw new InvalidCredentialsException();

            var token = _jwtService.GenerateToken(faculty.Id, faculty.Email, faculty.Name, "FACULTY");

            return new JwtResponseDTO
            {
                Token = token,
                Type = "Bearer",
                UserId = faculty.Id,
                Email = faculty.Email,
                Name = faculty.Name,
                Role = "FACULTY"
            };
        }

        // ── Admin Login ──
        public async Task<JwtResponseDTO> LoginAdminAsync(LoginDTO dto)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email)
                ?? throw new InvalidCredentialsException();

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
                throw new InvalidCredentialsException();

            var token = _jwtService.GenerateToken(admin.Id, admin.Email, admin.Name, "ADMIN");

            return new JwtResponseDTO
            {
                Token = token,
                Type = "Bearer",
                UserId = admin.Id,
                Email = admin.Email,
                Name = admin.Name,
                Role = "ADMIN"
            };
        }

        // ── Forgot Password ──
        public async Task<string> ForgotPasswordAsync(ForgotPasswordDTO dto)
        {
            string userType = "";
            if (await _context.Students.AnyAsync(s => s.Email == dto.Email))
                userType = "STUDENT";
            else if (await _context.Faculty.AnyAsync(f => f.Email == dto.Email))
                userType = "FACULTY";
            else if (await _context.Admins.AnyAsync(a => a.Email == dto.Email))
                userType = "ADMIN";
            else
                throw new NotFoundException("No account found with this email");

            var token = Guid.NewGuid().ToString("N");

            var resetToken = new PasswordResetToken
            {
                Email = dto.Email,
                Token = token,
                UserType = userType,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // In production, send email with reset link containing the token
            // For demo purposes, returning the token directly
            return $"Password reset token generated. Token: {token} (In production, this would be sent via email)";
        }

        // ── Reset Password ──
        public async Task<string> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == dto.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                ?? throw new Exceptions.ValidationException("Invalid or expired reset token");

            ValidatePassword(dto.NewPassword);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            switch (resetToken.UserType)
            {
                case "STUDENT":
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == resetToken.Email)
                        ?? throw new NotFoundException("Student not found");
                    student.PasswordHash = passwordHash;
                    student.UpdatedAt = DateTime.UtcNow;
                    break;

                case "FACULTY":
                    var faculty = await _context.Faculty.FirstOrDefaultAsync(f => f.Email == resetToken.Email)
                        ?? throw new NotFoundException("Faculty not found");
                    faculty.PasswordHash = passwordHash;
                    break;

                case "ADMIN":
                    var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == resetToken.Email)
                        ?? throw new NotFoundException("Admin not found");
                    admin.PasswordHash = passwordHash;
                    break;
            }

            resetToken.IsUsed = true;
            await _context.SaveChangesAsync();

            return "Password has been reset successfully";
        }

        // ── Change Password ──
        public async Task<string> ChangePasswordAsync(int userId, string role, ChangePasswordDTO dto)
        {
            ValidatePassword(dto.NewPassword);

            switch (role)
            {
                case "STUDENT":
                    var student = await _context.Students.FindAsync(userId)
                        ?? throw new NotFoundException("Student not found");
                    if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, student.PasswordHash))
                        throw new InvalidCredentialsException("Current password is incorrect");
                    student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    student.UpdatedAt = DateTime.UtcNow;
                    break;

                case "FACULTY":
                    var faculty = await _context.Faculty.FindAsync(userId)
                        ?? throw new NotFoundException("Faculty not found");
                    if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, faculty.PasswordHash))
                        throw new InvalidCredentialsException("Current password is incorrect");
                    faculty.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    break;

                case "ADMIN":
                    var admin = await _context.Admins.FindAsync(userId)
                        ?? throw new NotFoundException("Admin not found");
                    if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, admin.PasswordHash))
                        throw new InvalidCredentialsException("Current password is incorrect");
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    break;

                default:
                    throw new UnauthorizedException("Invalid role");
            }

            await _context.SaveChangesAsync();
            return "Password changed successfully";
        }

        // ── Helpers ──
        private static void ValidatePassword(string password)
        {
            if (password.Length < 8)
                throw new Exceptions.ValidationException("Password must be at least 8 characters long");

            if (!password.Any(char.IsUpper))
                throw new Exceptions.ValidationException("Password must contain at least 1 uppercase letter");

            if (!password.Any(char.IsDigit))
                throw new Exceptions.ValidationException("Password must contain at least 1 number");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                throw new Exceptions.ValidationException("Password must contain at least 1 special character");
        }

        private string GenerateRollNumber(string branch, int year)
        {
            var count = _context.Students.Count(s => s.Branch == branch && s.EnrollmentYear == year);
            return $"{year}{branch}{(count + 1):D3}";
        }
    }
}
