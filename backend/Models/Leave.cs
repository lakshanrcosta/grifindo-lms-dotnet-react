using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using grifindo_lms_api.Enums;

namespace grifindo_lms_api.Models
{
    public class Leave
    {
        [Key]
        public int LeaveId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public LeaveStatus Status { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public double Duration { get; set; }

        // Navigation property to User
        public User? User { get; set; }
    }
}
