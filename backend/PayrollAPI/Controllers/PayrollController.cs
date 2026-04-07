using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize(Roles = "Admin")]
public class PayrollController(AppDbContext db) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GeneratePayrollRequest req)
    {
        var alreadyExists = await db.PayrollRecords
            .AnyAsync(p => p.EmployeeId == req.EmployeeId
                        && p.Month == req.Month
                        && p.Year == req.Year);

        if (alreadyExists)
            return BadRequest(new { message = "Payroll already generated for this month" });

        var employee = await db.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == req.EmployeeId);
        if (employee == null) return NotFound(new { message = "Employee not found" });

        // Working days = all days in month except Sundays (simple rule)
        var daysInMonth = DateTime.DaysInMonth(req.Year, req.Month);
        var workingDays = Enumerable.Range(1, daysInMonth)
            .Count(d => new DateTime(req.Year, req.Month, d).DayOfWeek != DayOfWeek.Sunday);

        var attendanceRecords = await db.Attendances
            .Where(a => a.EmployeeId == req.EmployeeId
                     && a.Date.Month == req.Month
                     && a.Date.Year == req.Year)
            .ToListAsync();

        var presentDays = attendanceRecords.Count(a => a.Status == "Present")
                        + (int)(attendanceRecords.Count(a => a.Status == "HalfDay") * 0.5);
        var absentDays = workingDays - presentDays;
        if (absentDays < 0) absentDays = 0;

        var perDaySalary = employee.MonthlySalary / workingDays;
        var absentDeduction = Math.Round(perDaySalary * absentDays, 2);
        var netSalary = employee.MonthlySalary - absentDeduction - req.ManualDeduction;

        var payroll = new PayrollRecord
        {
            EmployeeId = req.EmployeeId,
            Month = req.Month,
            Year = req.Year,
            GrossSalary = employee.MonthlySalary,
            WorkingDays = workingDays,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            AbsentDeduction = absentDeduction,
            ManualDeduction = req.ManualDeduction,
            DeductionNote = req.DeductionNote,
            NetSalary = netSalary
        };

        db.PayrollRecords.Add(payroll);
        await db.SaveChangesAsync();

        return Ok(ToResponse(payroll, employee.User.Name));
    }

    [HttpGet("{employeeId}/{month}/{year}")]
    [Authorize] // Both Admin and Employee
    public async Task<IActionResult> Get(int employeeId, int month, int year)
    {
        var payroll = await db.PayrollRecords
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId
                                   && p.Month == month
                                   && p.Year == year);

        if (payroll == null) return NotFound(new { message = "Payroll not found" });
        return Ok(ToResponse(payroll, payroll.Employee.User.Name));
    }

    private static PayrollResponse ToResponse(PayrollRecord p, string name) => new(
        p.Id, p.EmployeeId, name, p.Month, p.Year,
        p.GrossSalary, p.WorkingDays, p.PresentDays, p.AbsentDays,
        p.AbsentDeduction, p.ManualDeduction, p.DeductionNote,
        p.NetSalary, p.Status
    );
}
