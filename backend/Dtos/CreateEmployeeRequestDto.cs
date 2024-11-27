using grifindo_lms_api.Enums;

namespace grifindo_lms_api.Dtos
{
    public class CreateEmployeeRequestDto
    {
        public string EmployeeNumber { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Role { get; set; } = default!;
        public DateTime DateOfJoining { get; set; }
        public bool IsPermanent { get; set; }
    }
}
