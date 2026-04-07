using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Services;

public class AttendanceService(AppDbContext db)
{
    public async Task<(bool Success, string Message, int Id)> MarkAsync(MarkAttendanceRequest req)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == req.EmployeeId && e.IsActive);
        if (employee == null)
            return (false, "Employee not found or inactive", 0);

        var exists = await db.Attendances
            .AnyAsync(a => a.EmployeeId == req.EmployeeId && a.Date == req.Date);

        if (exists)
            return (false, "Attendance already marked for this date", 0);

        var attendance = new Attendance
        {
            EmployeeId = req.EmployeeId,
            Date = req.Date,
            Month = req.Date.Month,
            Year = req.Date.Year,
            Status = req.Status,
            CheckIn = req.CheckIn,
            CheckOut = req.CheckOut,
            Note = req.Note
        };

        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();
        return (true, "Attendance marked", attendance.Id);
    }

    public async Task<MonthlySummaryResponse> GetMonthlyAsync(int employeeId, int month, int year)
    {
        var records = await db.Attendances
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Where(a => a.EmployeeId == employeeId && a.Month == month && a.Year == year)
            .OrderBy(a => a.Date)
            .Select(a => new AttendanceResponse(
                a.Id, a.EmployeeId, a.Employee.User.Name,
                a.Date, a.Status, a.CheckIn, a.CheckOut, a.Note))
            .ToListAsync();

        return new MonthlySummaryResponse(
            employeeId, month, year,
            records.Count(r => r.Status == "Present"),
            records.Count(r => r.Status == "Absent"),
            records.Count(r => r.Status == "HalfDay"),
            records);
    }
}
