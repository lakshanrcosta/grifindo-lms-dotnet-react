using grifindo_lms_api.Enums;

namespace grifindo_lms_api.Dtos
{
    public class ApplyLeaveDto
    {
        public required string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
