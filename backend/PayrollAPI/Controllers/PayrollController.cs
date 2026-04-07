using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs;
using PayrollAPI.Services;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize(Roles = "Admin")]
public class PayrollController(PayrollService svc) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GeneratePayrollRequest req)
    {
        var (success, message, data) = await svc.GenerateAsync(req);
        if (!success) return BadRequest(new { message });
        return Ok(data);
    }

    [HttpGet("{employeeId}/{month}/{year}")]
    [Authorize]
    public async Task<IActionResult> Get(int employeeId, int month, int year)
    {
        var result = await svc.GetAsync(employeeId, month, year);
        if (result == null) return NotFound(new { message = "Payroll not found" });
        return Ok(result);
    }
}
