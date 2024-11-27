using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grifindo_lms_api.Models
{
    public class WorkSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan RoasterStartTime { get; set; }

        [Required]
        public TimeSpan RoasterEndTime { get; set; }

        // Navigation property to User
        public User? User { get; set; }
    }
}