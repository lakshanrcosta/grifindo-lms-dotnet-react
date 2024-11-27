using grifindo_lms_api.Data;
using grifindo_lms_api.Dtos;
using grifindo_lms_api.Enums;
using grifindo_lms_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace grifindo_lms_api.Controllers
{
    [ApiController]
    [Route("api/admin/employees")]
    [Authorize] // Ensure the user is authenticated
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] CreateEmployeeRequestDto request)
        {
            // Extract the user's role from the JWT token
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRoleClaim) || !Enum.TryParse<UserRole>(userRoleClaim, out var userRole))
            {
                return Unauthorized(new { message = "Invalid or missing role in token." });
            }

            // Only Admin users are allowed to create employees
            if (userRole != UserRole.Admin)
            {
                return Unauthorized(new { message = "Access denied. Only Admins can create employees." });

            }

            // Check if the employee number or email already exists
            if (_context.Users.Any(u => u.EmployeeNumber == request.EmployeeNumber || u.Email == request.Email))
            {
                return Conflict(new { message = "Employee with the same Employee Number or Email already exists." });
            }

            // Hash the password
            var hashedPassword = HashPassword(request.Password);

            // Create the new employee
            var newEmployee = new User
            {
                EmployeeNumber = request.EmployeeNumber,
                Name = request.Name,
                Email = request.Email,
                Password = hashedPassword,
                Role = Enum.Parse<UserRole>(request.Role),
                DateOfJoining = request.DateOfJoining,
                IsPermanent = request.IsPermanent
            };

            _context.Users.Add(newEmployee);
            _context.SaveChanges();

            return Ok(new { message = "Employee created successfully" });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
