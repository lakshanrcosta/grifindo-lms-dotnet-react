using grifindo_lms_api.Data;
using grifindo_lms_api.Models;
using grifindo_lms_api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using grifindo_lms_api.Enums;

namespace grifindo_lms_api.Controllers
{
    [ApiController]
    [Route("api/employee")]
    [Authorize] // Ensure the user is authenticated
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("{employeeId}/leaves")]
        public IActionResult ApplyLeave(int employeeId, [FromBody] ApplyLeaveDto request)
        {
            // Retrieve authenticated user's ID from JWT claims
            var userIdClaim = User.FindFirst("userId");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Unauthorized(new { message = "User ID is not present in the token." });
            }

            if (!int.TryParse(userIdClaim.Value, out var authenticatedUserId))
            {
                return BadRequest(new { message = "Invalid User ID format in the token." });
            }

            // Ensure the authenticated user is only accessing their own data
            if (authenticatedUserId != employeeId)
            {
                return Conflict(new { message = "You are not authorized to access this data." });
            }

            // Validate User Authentication
            var user = _context.Users.FirstOrDefault(u => u.UserId == employeeId);

            if (user == null)
                return Unauthorized(new { message = "User is not authenticated" });


            // Fetch Leave Entitlements
            var entitlements = _context.LeaveEntitlements.FirstOrDefault(e => e.UserId == employeeId);
            if (entitlements == null)
                return NotFound(new { message = "Leave entitlements not found" });

            // Validate if the employee has overlapping leaves
            var overlappingLeave = _context.Leaves.Any(l =>
                l.UserId == employeeId &&
                ((l.StartDate <= request.EndDate && l.EndDate >= request.StartDate) ||   // Overlapping date range
                 (request.StartDate <= l.EndDate && request.EndDate >= l.StartDate)) && // Reverse overlap check
                l.Status == LeaveStatus.Approved); // Only check approved leaves

            if (overlappingLeave)
                return BadRequest(new { message = "Leave request overlaps with existing approved leave" });

            // Fetch Roaster Information (if needed)
            var workSchedule = _context.WorkSchedules.FirstOrDefault(ws => ws.UserId == employeeId);
            var roasterStartTime = workSchedule?.RoasterStartTime;

            try
            {
                // Attempt to parse the LeaveType
                var leaveType = Enum.Parse<LeaveType>(request.LeaveType, true); // Case-insensitive parsing

                switch (leaveType)
                {
                    case LeaveType.Annual:
                        return HandleAnnualLeave(request, entitlements, employeeId);

                    case LeaveType.Casual:
                        return HandleCasualLeave(request, entitlements, roasterStartTime, employeeId);

                    case LeaveType.Short:
                        return HandleShortLeave(request, entitlements, employeeId);

                    default:
                        return BadRequest(new { message = "Invalid Leave Type" });
                }
            }
            catch (ArgumentException)
            {
                // Catch parsing error and return a bad request response
                return BadRequest(new { message = $"Invalid Leave Type: {request.LeaveType}" });
            }
        }

        [HttpGet("{employeeId}/leaves")]
        public IActionResult GetLeaveHistory(int employeeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {

            // Retrieve authenticated user's ID from JWT claims
            var userIdClaim = User.FindFirst("userId");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Unauthorized(new { message = "User ID is not present in the token." });
            }

            if (!int.TryParse(userIdClaim.Value, out var authenticatedUserId))
            {
                return BadRequest(new { message = "Invalid User ID format in the token." });
            }

            // Ensure the authenticated user is only accessing their own data
            if (authenticatedUserId != employeeId)
            {
                return Conflict(new { message = "You are not authorized to access this data." });
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
            // Retrieve authenticated user's ID from JWT claims
            var userIdClaim = User.FindFirst("userId");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Unauthorized(new { message = "User ID is not present in the token." });
            }

            if (!int.TryParse(userIdClaim.Value, out var authenticatedUserId))
            {
                return BadRequest(new { message = "Invalid User ID format in the token." });
            }

            // Ensure the authenticated user is only accessing their own data
            if (authenticatedUserId != employeeId)
            {
                return Conflict(new { message = "You are not authorized to access this data." });
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

            // Retrieve authenticated user's ID from JWT claims
            var userIdClaim = User.FindFirst("userId");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Unauthorized(new { message = "User ID is not present in the token." });
            }

            if (!int.TryParse(userIdClaim.Value, out var authenticatedUserId))
            {
                return BadRequest(new { message = "Invalid User ID format in the token." });
            }

            // Ensure the authenticated user is only accessing their own data
            if (authenticatedUserId != employeeId)
            {
                return Conflict(new { message = "You are not authorized to access this data." });
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

        [HttpGet("{employeeId}/leave-entitlements")]
        public IActionResult GetLeaveEntitlements(int employeeId)
        {
            // Retrieve authenticated user's ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User ID is not present in the token." });
            }

            if (!int.TryParse(userIdClaim, out var authenticatedUserId))
            {
                return BadRequest(new { message = "Invalid User ID format in the token." });
            }

            // Ensure the authenticated user is only accessing their own data
            if (authenticatedUserId != employeeId)
            {
                return Conflict(new { message = "You are not authorized to access this data." });
            }

            // Retrieve leave entitlements for the logged-in user
            var entitlements = _context.LeaveEntitlements.FirstOrDefault(e => e.UserId == authenticatedUserId);
            if (entitlements == null)
            {
                return NotFound(new { message = "Leave entitlements not found for the user." });
            }

            // Return the leave entitlements directly
            var leaveEntitlements = new
            {
                AnnualLeave = entitlements.AnnualLeave,
                CasualLeave = entitlements.CasualLeave,
                ShortLeave = entitlements.ShortLeave
            };

            return Ok(leaveEntitlements);
        }

        private IActionResult HandleAnnualLeave(ApplyLeaveDto request, LeaveEntitlement entitlements, int employeeId)
        {
            var daysRequested = (request.EndDate - request.StartDate).TotalDays + 1;

            if (daysRequested > entitlements.AnnualLeave || request.StartDate < DateTime.UtcNow.AddDays(7))
                return BadRequest(new { message = "Invalid Annual Leave request" });

            SubmitLeave(request, employeeId, "Pending");
            entitlements.AnnualLeave -= (int)daysRequested;
            _context.SaveChanges();

            return Ok(new { message = "Annual Leave submitted successfully" });
        }

        private IActionResult HandleCasualLeave(ApplyLeaveDto request, LeaveEntitlement entitlements, TimeSpan? roasterStartTime, int employeeId)
        {
            var daysRequested = (request.EndDate - request.StartDate).TotalDays + 1;

            if (daysRequested > entitlements.CasualLeave || (roasterStartTime.HasValue && request.StartDate >= DateTime.UtcNow.Date + roasterStartTime.Value))
                return BadRequest(new { message = "Invalid Casual Leave request" });

            SubmitLeave(request, employeeId, "Pending");
            entitlements.CasualLeave -= (int)daysRequested;
            _context.SaveChanges();

            return Ok(new { message = "Casual Leave submitted successfully" });
        }

        private IActionResult HandleShortLeave(ApplyLeaveDto request, LeaveEntitlement entitlements, int employeeId)
        {
            var leaveDuration = (request.EndDate - request.StartDate).TotalHours;

            if (leaveDuration > 1.5 || entitlements.ShortLeave < 1)
                return BadRequest(new { message = "Invalid Short Leave request" });

            SubmitLeave(request, employeeId, "Pending");
            entitlements.ShortLeave -= 1;
            _context.SaveChanges();

            return Ok(new { message = "Short Leave submitted successfully" });
        }

        private void SubmitLeave(ApplyLeaveDto request, int employeeId, string status)
        {
            var leave = new Leave
            {
                UserId = employeeId,
                LeaveType = Enum.Parse<LeaveType>(request.LeaveType),
                Status = Enum.Parse<LeaveStatus>(status),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Duration = (request.EndDate - request.StartDate).TotalDays + 1
            };

            _context.Leaves.Add(leave);
        }
    }
}
