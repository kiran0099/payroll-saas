using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/leave")]
[Authorize]
public class LeaveController(AppDbContext db) : ControllerBase
{
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveRequest req)
    {
        var employee = await db.Employees.FindAsync(req.EmployeeId);
        if (employee == null) return NotFound(new { message = "Employee not found" });

        var leave = new LeaveRequest
        {
            EmployeeId = req.EmployeeId,
            LeaveType = req.LeaveType,
            FromDate = req.FromDate,
            ToDate = req.ToDate,
            Reason = req.Reason,
            Status = "Pending"
        };

        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();
        return Ok(new { leave.Id, message = "Leave applied" });
    }

    [HttpPost("approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve([FromBody] ApproveLeaveRequest req)
    {
        var leave = await db.LeaveRequests
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(l => l.Id == req.LeaveRequestId);

        if (leave == null) return NotFound();

        leave.Status = req.Status;
        leave.AdminNote = req.AdminNote;
        leave.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(new LeaveResponse(
            leave.Id, leave.EmployeeId, leave.Employee.User.Name,
            leave.LeaveType, leave.FromDate, leave.ToDate,
            leave.Reason, leave.Status, leave.AdminNote, leave.CreatedAt));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? employeeId, [FromQuery] string? status)
    {
        var query = db.LeaveRequests.Include(l => l.Employee).ThenInclude(e => e.User).AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status == status);

        var result = await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LeaveResponse(
                l.Id, l.EmployeeId, l.Employee.User.Name,
                l.LeaveType, l.FromDate, l.ToDate,
                l.Reason, l.Status, l.AdminNote, l.CreatedAt))
            .ToListAsync();

        return Ok(result);
    }
}
