namespace grifindo_lms_api.Dtos

{
    public class SetLeaveEntitlementDto
    {
        public required string EmployeeNumber { get; set; }
        public int AnnualLeave { get; set; }
        public int CasualLeave { get; set; }
        public int ShortLeave { get; set; }
    }

}