using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await db.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive)
            .Select(e => ToResponse(e))
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email already exists" });

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = "Employee"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var employee = new Employee
        {
            UserId = user.Id,
            Department = req.Department,
            Designation = req.Designation,
            MonthlySalary = req.MonthlySalary,
            JoiningDate = req.JoiningDate
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        employee.User = user;
        return CreatedAtAction(nameof(GetAll), new { id = employee.Id }, ToResponse(employee));
    }

    [HttpPost("update/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest req)
    {
        var employee = await db.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null) return NotFound();

        employee.User.Name = req.Name;
        employee.User.UpdatedAt = DateTime.UtcNow;
        employee.Department = req.Department;
        employee.Designation = req.Designation;
        employee.MonthlySalary = req.MonthlySalary;
        employee.IsActive = req.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(ToResponse(employee));
    }

    [HttpPost("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        // Soft delete
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static EmployeeResponse ToResponse(Employee e) => new(
        e.Id, e.UserId, e.User.Name, e.User.Email,
        e.Department, e.Designation, e.MonthlySalary,
        e.JoiningDate, e.IsActive
    );
}
