namespace grifindo_lms_api.Dtos
{
    public class UpdateLeaveStatusDto
    {
        public string Status { get; set; } = string.Empty; // Expected values: "Approved" or "Rejected"
    }
}
