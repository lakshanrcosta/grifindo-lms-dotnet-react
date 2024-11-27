namespace grifindo_lms_api.Dtos
{
    public class LoginRequestDto
    {
        public required string EmployeeNumber { get; set; }
        public required string Password { get; set; }
    }
}
