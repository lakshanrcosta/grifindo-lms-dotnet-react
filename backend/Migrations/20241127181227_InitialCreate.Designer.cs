﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using grifindo_lms_api.Data;

#nullable disable

namespace grifindo_lms_api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241127181227_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("grifindo_lms_api.Models.Leave", b =>
                {
                    b.Property<int>("LeaveId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LeaveId"));

                    b.Property<double>("Duration")
                        .HasColumnType("float");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LeaveType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LeaveId");

                    b.HasIndex("UserId");

                    b.ToTable("Leaves");
                });

            modelBuilder.Entity("grifindo_lms_api.Models.LeaveEntitlement", b =>
                {
                    b.Property<int>("LeaveEntitlementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LeaveEntitlementId"));

                    b.Property<int>("AnnualLeave")
                        .HasColumnType("int");

                    b.Property<int>("CasualLeave")
                        .HasColumnType("int");

                    b.Property<int>("ShortLeave")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LeaveEntitlementId");

                    b.HasIndex("UserId");

                    b.ToTable("LeaveEntitlements");
                });

            modelBuilder.Entity("grifindo_lms_api.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<DateTime>("DateOfJoining")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("EmployeeNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsPermanent")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserId = 1,
                            DateOfJoining = new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "admin@example.com",
                            EmployeeNumber = "EMP001",
                            IsPermanent = true,
                            Name = "Admin User",
                            Password = "BTl6fVG9nj7zanzlEq7o6jpgN7oxfvVZ71L+bUZZhsA=",
                            Role = "Admin"
                        },
                        new
                        {
                            UserId = 2,
                            DateOfJoining = new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "employee@example.com",
                            EmployeeNumber = "EMP002",
                            IsPermanent = true,
                            Name = "Employee User",
                            Password = "BTl6fVG9nj7zanzlEq7o6jpgN7oxfvVZ71L+bUZZhsA=",
                            Role = "Employee"
                        });
                });

            modelBuilder.Entity("grifindo_lms_api.Models.WorkSchedule", b =>
                {
                    b.Property<int>("ScheduleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ScheduleId"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("RoasterEndTime")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("RoasterStartTime")
                        .HasColumnType("time");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("ScheduleId");

                    b.HasIndex("UserId");

                    b.ToTable("WorkSchedules");
                });

            modelBuilder.Entity("grifindo_lms_api.Models.Leave", b =>
                {
                    b.HasOne("grifindo_lms_api.Models.User", "User")
                        .WithMany("Leaves")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("grifindo_lms_api.Models.LeaveEntitlement", b =>
                {
                    b.HasOne("grifindo_lms_api.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("grifindo_lms_api.Models.WorkSchedule", b =>
                {
                    b.HasOne("grifindo_lms_api.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("grifindo_lms_api.Models.User", b =>
                {
                    b.Navigation("Leaves");
                });
#pragma warning restore 612, 618
        }
    }
}
