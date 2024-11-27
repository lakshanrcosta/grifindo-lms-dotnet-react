using grifindo_lms_api.Data;
using grifindo_lms_api.Models;
using grifindo_lms_api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("leaves")]
        public IActionResult ApplyLeave([FromBody] ApplyLeaveDto request)
        {
            // Validate User Authentication
            var user = _context.Users.FirstOrDefault(u => u.UserId == request.EmployeeId);
            if (user == null)
                return Unauthorized(new { message = "User is not authenticated" });

            // Fetch Leave Entitlements
            var entitlements = _context.LeaveEntitlements.FirstOrDefault(e => e.UserId == request.EmployeeId);
            if (entitlements == null)
                return NotFound(new { message = "Leave entitlements not found" });

            // Validate if the employee has overlapping leaves
            var overlappingLeave = _context.Leaves.Any(l =>
                l.UserId == request.EmployeeId &&
                ((l.StartDate <= request.EndDate && l.EndDate >= request.StartDate) ||   // Overlapping date range
                 (request.StartDate <= l.EndDate && request.EndDate >= l.StartDate)) && // Reverse overlap check
                l.Status == LeaveStatus.Approved); // Only check approved leaves

            if (overlappingLeave)
                return BadRequest(new { message = "Leave request overlaps with existing approved leave" });

            // Fetch Roaster Information (if needed)
            var workSchedule = _context.WorkSchedules.FirstOrDefault(ws => ws.UserId == request.EmployeeId);
            var roasterStartTime = workSchedule?.RoasterStartTime;

            // Validate Leave Type and Apply Logic
            switch (Enum.Parse<LeaveType>(request.LeaveType))
            {
                case LeaveType.Annual:
                    return HandleAnnualLeave(request, entitlements);

                case LeaveType.Casual:
                    return HandleCasualLeave(request, entitlements, roasterStartTime);

                case LeaveType.Short:
                    return HandleShortLeave(request, entitlements);

                default:
                    return BadRequest(new { message = "Invalid Leave Type" });
            }
        }

        private IActionResult HandleAnnualLeave(ApplyLeaveDto request, LeaveEntitlement entitlements)
        {
            var daysRequested = (request.EndDate - request.StartDate).TotalDays + 1;

            if (daysRequested > entitlements.AnnualLeave || request.StartDate < DateTime.UtcNow.AddDays(7))
                return BadRequest(new { message = "Invalid Annual Leave request" });

            SubmitLeave(request, "Pending");
            entitlements.AnnualLeave -= (int)daysRequested;
            _context.SaveChanges();

            return Ok(new { message = "Annual Leave submitted successfully" });
        }

        private IActionResult HandleCasualLeave(ApplyLeaveDto request, LeaveEntitlement entitlements, TimeSpan? roasterStartTime)
        {
            var daysRequested = (request.EndDate - request.StartDate).TotalDays + 1;

            if (daysRequested > entitlements.CasualLeave || (roasterStartTime.HasValue && request.StartDate >= DateTime.UtcNow.Date + roasterStartTime.Value))
                return BadRequest(new { message = "Invalid Casual Leave request" });

            SubmitLeave(request, "Pending");
            entitlements.CasualLeave -= (int)daysRequested;
            _context.SaveChanges();

            return Ok(new { message = "Casual Leave submitted successfully" });
        }

        private IActionResult HandleShortLeave(ApplyLeaveDto request, LeaveEntitlement entitlements)
        {
            var leaveDuration = (request.EndDate - request.StartDate).TotalHours;

            if (leaveDuration > 1.5 || entitlements.ShortLeave < 1)
                return BadRequest(new { message = "Invalid Short Leave request" });

            SubmitLeave(request, "Pending");
            entitlements.ShortLeave -= 1;
            _context.SaveChanges();

            return Ok(new { message = "Short Leave submitted successfully" });
        }

        private void SubmitLeave(ApplyLeaveDto request, string status)
        {
            var leave = new Leave
            {
                UserId = request.EmployeeId,
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
