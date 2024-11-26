using Microsoft.EntityFrameworkCore;
using grifindo_lms_api.Models;

namespace grifindo_lms_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveEntitlement> LeaveEntitlements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Leave>()
                .Property(l => l.LeaveType)
                .HasConversion<string>();

            modelBuilder.Entity<Leave>()
                .Property(l => l.Status)
                .HasConversion<string>();
        }
    }
}