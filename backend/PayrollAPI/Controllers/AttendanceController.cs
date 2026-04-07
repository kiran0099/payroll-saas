using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/attendance")]
[Authorize]
public class AttendanceController(AppDbContext db) : ControllerBase
{
    [HttpPost("mark")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Mark([FromBody] MarkAttendanceRequest req)
    {
        var exists = await db.Attendances
            .AnyAsync(a => a.EmployeeId == req.EmployeeId && a.Date == req.Date);

        if (exists)
            return BadRequest(new { message = "Attendance already marked for this date" });

        var attendance = new Attendance
        {
            EmployeeId = req.EmployeeId,
            Date = req.Date,
            Status = req.Status,
            CheckIn = req.CheckIn,
            CheckOut = req.CheckOut,
            Note = req.Note
        };

        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();

        return Ok(new { attendance.Id, message = "Attendance marked" });
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly([FromQuery] int employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        var records = await db.Attendances
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Where(a => a.EmployeeId == employeeId
                     && a.Date.Month == month
                     && a.Date.Year == year)
            .OrderBy(a => a.Date)
            .Select(a => new AttendanceResponse(
                a.Id, a.EmployeeId, a.Employee.User.Name,
                a.Date, a.Status, a.CheckIn, a.CheckOut, a.Note))
            .ToListAsync();

        var summary = new
        {
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            TotalRecords = records.Count,
            PresentDays = records.Count(r => r.Status == "Present"),
            AbsentDays = records.Count(r => r.Status == "Absent"),
            HalfDays = records.Count(r => r.Status == "HalfDay"),
            Records = records
        };

        return Ok(summary);
    }
}
