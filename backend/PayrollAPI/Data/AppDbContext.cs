using Microsoft.EntityFrameworkCore;
using PayrollAPI.Models;

namespace PayrollAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        // Employee → User (1:1)
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasOne(emp => emp.User)
             .WithOne(u => u.Employee)
             .HasForeignKey<Employee>(emp => emp.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(emp => emp.MonthlySalary).HasColumnType("decimal(18,2)");
        });

        // Attendance → Employee
        modelBuilder.Entity<Attendance>(e =>
        {
            e.HasOne(a => a.Employee)
             .WithMany(emp => emp.Attendances)
             .HasForeignKey(a => a.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate attendance for same employee+date
            e.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
        });

        // LeaveRequest → Employee
        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasOne(l => l.Employee)
             .WithMany(emp => emp.LeaveRequests)
             .HasForeignKey(l => l.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // PayrollRecord → Employee
        modelBuilder.Entity<PayrollRecord>(e =>
        {
            e.HasOne(p => p.Employee)
             .WithMany(emp => emp.PayrollRecords)
             .HasForeignKey(p => p.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(p => p.GrossSalary).HasColumnType("decimal(18,2)");
            e.Property(p => p.AbsentDeduction).HasColumnType("decimal(18,2)");
            e.Property(p => p.ManualDeduction).HasColumnType("decimal(18,2)");
            e.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");

            // One payroll per employee per month/year
            e.HasIndex(p => new { p.EmployeeId, p.Month, p.Year }).IsUnique();
        });
    }
}
