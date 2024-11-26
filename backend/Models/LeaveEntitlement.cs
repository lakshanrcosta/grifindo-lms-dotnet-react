using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grifindo_lms_api.Models
{
    public class LeaveEntitlement
    {
        [Key]
        public int LeaveEntitlementId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        public int AnnualLeave { get; set; }

        [Required]
        public int CasualLeave { get; set; }

        [Required]
        public int ShortLeave { get; set; }

        // Navigation property to User
        public User? User { get; set; }
    }
}
