using grifindo_lms_api.Enums;
using grifindo_lms_api.Models;
using Microsoft.EntityFrameworkCore;

namespace grifindo_lms_api.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    EmployeeNumber = "EMP001",
                    Password = "BTl6fVG9nj7zanzlEq7o6jpgN7oxfvVZ71L+bUZZhsA=", // Pre-hashed password
                    Role = UserRole.Admin,
                    Name = "Admin User",
                    Email = "admin@example.com",
                    DateOfJoining = new DateTime(2023, 1, 1), // Fixed date
                    IsPermanent = true
                },
                new User
                {
                    UserId = 2,
                    EmployeeNumber = "EMP002",
                    Password = "BTl6fVG9nj7zanzlEq7o6jpgN7oxfvVZ71L+bUZZhsA=", // Pre-hashed password
                    Role = UserRole.Employee,
                    Name = "Employee User",
                    Email = "employee@example.com",
                    DateOfJoining = new DateTime(2023, 1, 1), // Fixed date
                    IsPermanent = true
                }
            );
        }
    }
}
