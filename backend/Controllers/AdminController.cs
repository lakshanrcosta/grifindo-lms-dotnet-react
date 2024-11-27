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
    [Route("api/admin")]
    [Authorize] // Ensure the user is authenticated
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("employees")]
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


        [HttpPost("leave-entitlements")]
        public IActionResult SetLeaveEntitlement([FromBody] SetLeaveEntitlementDto request)
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
                return Unauthorized(new { message = "Access denied. Only Admins can set leave entitlements." });

            }

            // Check if user exists
            var user = _context.Users.FirstOrDefault(u => u.EmployeeNumber == request.EmployeeNumber);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if entitlement already exists
            var existingEntitlement = _context.LeaveEntitlements.FirstOrDefault(le => le.UserId == user.UserId);
            if (existingEntitlement != null)
            {
                return BadRequest(new { message = "Leave entitlement already exists for this user" });
            }

            // Create new entitlement
            var leaveEntitlement = new LeaveEntitlement
            {
                UserId = user.UserId,
                AnnualLeave = request.AnnualLeave,
                CasualLeave = request.CasualLeave,
                ShortLeave = request.ShortLeave
            };

            _context.LeaveEntitlements.Add(leaveEntitlement);
            _context.SaveChanges();

            return Ok(new { message = "Leave entitlements set successfully" });
        }

        [HttpPut("leaves/{leaveId}/status")]
        public IActionResult UpdateLeaveStatus(int leaveId, [FromBody] UpdateLeaveStatusDto request)
        {
            // Extract the user's role from the JWT token
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRoleClaim) || !Enum.TryParse<UserRole>(userRoleClaim, out var userRole))
            {
                return Unauthorized(new { message = "Invalid or missing role in token." });
            }

            // Only Admin users are allowed to update leave status
            if (userRole != UserRole.Admin)
            {
                return Unauthorized(new { message = "Access denied. Only Admins can update leave status." });
            }

            // Validate status
            if (!Enum.TryParse<LeaveStatus>(request.Status, true, out var leaveStatus))
            {
                return BadRequest(new { message = "Invalid status. Must be 'Approved' or 'Rejected'." });
            }

            // Find the leave request
            var leave = _context.Leaves.FirstOrDefault(l => l.LeaveId == leaveId);
            if (leave == null)
            {
                return NotFound(new { message = "Leave request not found." });
            }

            // Check if the status is already the same as the requested status
            if (leave.Status == leaveStatus)
            {
                return BadRequest(new { message = $"Leave is already {leaveStatus}." });
            }

            // If leave is being rejected, return the amount to leave entitlements
            if (leaveStatus == LeaveStatus.Rejected)
            {
                var entitlements = _context.LeaveEntitlements.FirstOrDefault(e => e.UserId == leave.UserId);
                if (entitlements == null)
                {
                    return NotFound(new { message = "Leave entitlements not found for the user." });
                }

                // Update the appropriate leave type balance
                switch (leave.LeaveType)
                {
                    case LeaveType.Annual:
                        entitlements.AnnualLeave += (int)leave.Duration;
                        break;

                    case LeaveType.Casual:
                        entitlements.CasualLeave += (int)leave.Duration;
                        break;

                    case LeaveType.Short:
                        entitlements.ShortLeave += (int)leave.Duration;
                        break;

                    default:
                        return BadRequest(new { message = "Invalid leave type associated with the leave request." });
                }

                // Save entitlement changes
                _context.SaveChanges();
            }

            // Update the leave status
            leave.Status = leaveStatus;
            _context.SaveChanges();

            return Ok(new { message = "Leave status updated." });
        }


        [HttpGet("leaves")]
        public IActionResult GetAllEmployeeLeaves([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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
                return Unauthorized(new { message = "Access denied. Only Admins can view all employee leaves." });

            }

            // Query leave history
            var leaveHistoryQuery = _context.Leaves
                .Join(_context.Users,
                    leave => leave.UserId,
                    user => user.UserId,
                    (leave, user) => new
                    {
                        user.Name,
                        leave.LeaveType,
                        leave.Status,
                        leave.StartDate,
                        leave.EndDate,
                        leave.Duration
                    });

            // Apply optional date filters
            if (startDate.HasValue)
            {
                leaveHistoryQuery = leaveHistoryQuery.Where(l => l.StartDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                leaveHistoryQuery = leaveHistoryQuery.Where(l => l.EndDate <= endDate.Value);
            }

            // Execute query and project results
            var leaveHistory = leaveHistoryQuery
                .Select(l => new
                {
                    EmployeeName = l.Name,
                    LeaveType = l.LeaveType.ToString(),
                    Status = l.Status.ToString(),
                    StartDate = l.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = l.EndDate.ToString("yyyy-MM-dd"),
                    l.Duration
                })
                .ToList();

            return Ok(leaveHistory);
        }

        [HttpGet("{employeeId}/leaves")]
        public IActionResult GetLeaveHistory(int employeeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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
                return Unauthorized(new { message = "Access denied. Only Admins can view all employee leaves." });

            }

            // Check if the employee exists
            var employee = _context.Users.FirstOrDefault(u => u.UserId == employeeId);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            // Retrieve leave history for the employee
            var leaveHistoryQuery = _context.Leaves.Where(l => l.UserId == employeeId);

            // Apply optional date filters
            if (startDate.HasValue)
            {
                leaveHistoryQuery = leaveHistoryQuery.Where(l => l.StartDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                leaveHistoryQuery = leaveHistoryQuery.Where(l => l.EndDate <= endDate.Value);
            }

            // Execute query and project results
            var leaveHistory = leaveHistoryQuery
                .Select(l => new
                {
                    l.LeaveId,
                    LeaveType = l.LeaveType.ToString(),
                    Status = l.Status.ToString(),
                    StartDate = l.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = l.EndDate.ToString("yyyy-MM-dd"),
                    l.Duration
                })
                .ToList();

            return Ok(leaveHistory);
        }

        [HttpGet("{employeeId}/leaves/{leaveId}")]
        public IActionResult GetLeaveById(int employeeId, int leaveId)
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
                return Unauthorized(new { message = "Access denied. Only Admins can view all employee leaves." });

            }

            // Check if the employee exists
            var employee = _context.Users.FirstOrDefault(u => u.UserId == employeeId);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            // Retrieve the specific leave by leaveId
            var leave = _context.Leaves.FirstOrDefault(l => l.UserId == employeeId && l.LeaveId == leaveId);
            if (leave == null)
            {
                return NotFound(new { message = "Leave not found" });
            }

            // Project the result
            var leaveDetails = new
            {
                leave.LeaveId,
                LeaveType = leave.LeaveType.ToString(),
                Status = leave.Status.ToString(),
                StartDate = leave.StartDate.ToString("yyyy-MM-dd"),
                EndDate = leave.EndDate.ToString("yyyy-MM-dd"),
                leave.Duration
            };

            return Ok(leaveDetails);
        }


        [HttpDelete("{employeeId}/leaves/{leaveId}")]
        public IActionResult DeleteLeaveRequest(int employeeId, int leaveId)
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
                return Unauthorized(new { message = "Access denied. Only Admins can view employee leaves." });

            }

            // Find the leave request
            var leave = _context.Leaves.FirstOrDefault(l => l.LeaveId == leaveId && l.UserId == employeeId);
            if (leave == null)
            {
                return NotFound(new { message = "Leave request not found" });
            }

            // Check if the leave is in "Pending" status
            if (leave.Status != LeaveStatus.Pending)
            {
                return BadRequest(new { message = "Only leave requests in 'Pending' status can be deleted" });
            }

            // Delete the leave request
            _context.Leaves.Remove(leave);
            _context.SaveChanges();

            return Ok(new { message = "Leave request deleted successfully" });
        }

        [HttpPost("work-schedule")]
        public IActionResult SetWorkSchedule([FromBody] WorkScheduleRequestDto request)
        {
            // Extract the user's role from the JWT token
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRoleClaim) || !Enum.TryParse<UserRole>(userRoleClaim, out var userRole))
            {
                return Unauthorized(new { message = "Invalid or missing role in token." });
            }

            // Only Admin users are allowed to set work schedules
            if (userRole != UserRole.Admin)
            {
                return Unauthorized(new { message = "Access denied. Only Admins can set work schedules." });
            }

            // Check if the employee exists
            var employee = _context.Users.FirstOrDefault(u => u.UserId == request.EmployeeId);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            // Validate the date to ensure it's within the upcoming week (Monday-Sunday)
            var today = DateTime.UtcNow.Date;
            var nextMonday = today.AddDays(((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7);
            var nextSunday = nextMonday.AddDays(6);

            if (request.Date < nextMonday || request.Date > nextSunday)
            {
                return BadRequest(new { message = "Date must be within the upcoming week (Monday to Sunday)." });
            }

            // Validate for overlapping schedules
            var overlaps = _context.WorkSchedules.Any(ws =>
                ws.UserId == employee.UserId &&
                ws.Date == request.Date &&
                ((TimeSpan.Parse(request.RoasterStartTime) >= ws.RoasterStartTime &&
                  TimeSpan.Parse(request.RoasterStartTime) < ws.RoasterEndTime) ||
                 (TimeSpan.Parse(request.RoasterEndTime) > ws.RoasterStartTime &&
                  TimeSpan.Parse(request.RoasterEndTime) <= ws.RoasterEndTime)));

            if (overlaps)
            {
                return BadRequest(new { message = "Work schedule overlaps with an existing schedule for this employee on the given date." });
            }

            // Create and save the work schedule
            var workSchedule = new WorkSchedule
            {
                UserId = employee.UserId,
                Date = request.Date,
                RoasterStartTime = TimeSpan.Parse(request.RoasterStartTime),
                RoasterEndTime = TimeSpan.Parse(request.RoasterEndTime)
            };

            _context.WorkSchedules.Add(workSchedule);
            _context.SaveChanges();

            return Ok(new { message = "Work schedule set successfully" });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
