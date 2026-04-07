using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs;
using PayrollAPI.Services;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/attendance")]
[Authorize]
public class AttendanceController(AttendanceService svc) : ControllerBase
{
    [HttpPost("mark")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Mark([FromBody] MarkAttendanceRequest req)
    {
        var (success, message, id) = await svc.MarkAsync(req);
        if (!success) return BadRequest(new { message });
        return Ok(new { id, message });
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly([FromQuery] int employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        var result = await svc.GetMonthlyAsync(employeeId, month, year);
        return Ok(result);
    }
}
