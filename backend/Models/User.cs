using System.ComponentModel.DataAnnotations;
using grifindo_lms_api.Enums;

namespace grifindo_lms_api.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public required string EmployeeNumber { get; set; }

        [Required]
        [StringLength(100)]
        public required string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public required string Email { get; set; }

        [Required]
        public DateTime DateOfJoining { get; set; }

        public bool IsPermanent { get; set; }

        // Navigation property for one-to-many relationship with Leave
        public ICollection<Leave> Leaves { get; set; } = new List<Leave>();
    }
}
