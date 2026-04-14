using CollegePortal.API.DTOs.Auth;
using CollegePortal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegePortal.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Register a new student</summary>
        [HttpPost("student/register")]
        public async Task<IActionResult> RegisterStudent([FromBody] StudentRegisterDTO dto)
        {
            var result = await _authService.RegisterStudentAsync(dto);
            return Created("", result);
        }

        /// <summary>Register a new faculty member</summary>
        [HttpPost("faculty/register")]
        public async Task<IActionResult> RegisterFaculty([FromBody] FacultyRegisterDTO dto)
        {
            var result = await _authService.RegisterFacultyAsync(dto);
            return Created("", result);
        }

        /// <summary>Register a new admin (SuperAdmin only)</summary>
        [HttpPost("admin/register")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDTO dto)
        {
            var result = await _authService.RegisterAdminAsync(dto);
            return Created("", result);
        }

        /// <summary>Student login</summary>
        [HttpPost("student/login")]
        public async Task<IActionResult> LoginStudent([FromBody] LoginDTO dto)
        {
            var result = await _authService.LoginStudentAsync(dto);
            return Ok(result);
        }

        /// <summary>Faculty login</summary>
        [HttpPost("faculty/login")]
        public async Task<IActionResult> LoginFaculty([FromBody] LoginDTO dto)
        {
            var result = await _authService.LoginFacultyAsync(dto);
            return Ok(result);
        }

        /// <summary>Admin login</summary>
        [HttpPost("admin/login")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginDTO dto)
        {
            var result = await _authService.LoginAdminAsync(dto);
            return Ok(result);
        }

        /// <summary>Request password reset (sends token)</summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);
            return Ok(new { message = result });
        }

        /// <summary>Reset password using token</summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return Ok(new { message = result });
        }

        /// <summary>Change password (requires authentication)</summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var role = User.FindFirst("role")!.Value;
            var result = await _authService.ChangePasswordAsync(userId, role, dto);
            return Ok(new { message = result });
        }
    }
}
