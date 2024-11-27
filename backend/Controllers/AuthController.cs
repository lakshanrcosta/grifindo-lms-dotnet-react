using grifindo_lms_api.Data;
using grifindo_lms_api.Dtos;
using grifindo_lms_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace grifindo_lms_api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmployeeNumber == request.EmployeeNumber);

            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = JwtTokenService.GenerateToken(user.UserId, user.Role.ToString());

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("auth_token", token, cookieOptions);

            return Ok(new { role = user.Role.ToString(), userId = user.UserId, userName = user.Name, message = "Login successful" });
        }

        [HttpPost("logout")]
        [Authorize] // Ensure the user is authenticated
        public IActionResult Logout()
        {
            // Clear the authentication token cookie
            Response.Cookies.Append("auth_token", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1) // Set the cookie expiry to a past date
            });

            return Ok(new { message = "Logout successful" });
        }


        private static bool VerifyPassword(string inputPassword, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var inputHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            return Convert.ToBase64String(inputHash) == storedHash;
        }
    }
}
