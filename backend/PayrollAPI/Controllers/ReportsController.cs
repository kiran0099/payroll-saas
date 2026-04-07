using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    // GET /api/reports/attendance?month=4&year=2026
    [HttpGet("attendance")]
    public async Task<IActionResult> AttendanceReport([FromQuery] int month, [FromQuery] int year)
    {
        var data = await db.Employees
            .Include(e => e.User)
            .Include(e => e.Attendances)
            .Where(e => e.IsActive)
            .Select(e => new
            {
                EmployeeId = e.Id,
                Name = e.User.Name,
                Department = e.Department,
                Present = e.Attendances.Count(a => a.Date.Month == month && a.Date.Year == year && a.Status == "Present"),
                Absent = e.Attendances.Count(a => a.Date.Month == month && a.Date.Year == year && a.Status == "Absent"),
                HalfDay = e.Attendances.Count(a => a.Date.Month == month && a.Date.Year == year && a.Status == "HalfDay")
            })
            .ToListAsync();

        return Ok(new { Month = month, Year = year, Employees = data });
    }

    // GET /api/reports/salary?month=4&year=2026
    [HttpGet("salary")]
    public async Task<IActionResult> SalaryReport([FromQuery] int month, [FromQuery] int year)
    {
        var data = await db.PayrollRecords
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .Where(p => p.Month == month && p.Year == year)
            .Select(p => new
            {
                EmployeeId = p.EmployeeId,
                Name = p.Employee.User.Name,
                Department = p.Employee.Department,
                GrossSalary = p.GrossSalary,
                AbsentDeduction = p.AbsentDeduction,
                ManualDeduction = p.ManualDeduction,
                NetSalary = p.NetSalary,
                Status = p.Status
            })
            .ToListAsync();

        var totalNet = data.Sum(d => d.NetSalary);
        return Ok(new { Month = month, Year = year, TotalNetPayable = totalNet, Employees = data });
    }
}
