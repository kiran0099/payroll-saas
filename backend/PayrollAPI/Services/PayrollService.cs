using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Services;

public class PayrollService(AppDbContext db)
{
    private const int WorkingDaysPerMonth = 30;

    public async Task<(bool Success, string Message, PayrollResponse? Data)> GenerateAsync(GeneratePayrollRequest req)
    {
        var alreadyExists = await db.PayrollRecords
            .AnyAsync(p => p.EmployeeId == req.EmployeeId && p.Month == req.Month && p.Year == req.Year);

        if (alreadyExists)
            return (false, "Payroll already generated for this month", null);

        var employee = await db.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == req.EmployeeId && e.IsActive);

        if (employee == null)
            return (false, "Employee not found or inactive", null);

        var attendanceRecords = await db.Attendances
            .Where(a => a.EmployeeId == req.EmployeeId && a.Month == req.Month && a.Year == req.Year)
            .ToListAsync();

        var totalMarkedDays = attendanceRecords.Count;
        var presentDays = attendanceRecords.Count(a => a.Status == "Present")
                        + (int)(attendanceRecords.Count(a => a.Status == "HalfDay") * 0.5);

        var absentDays = Math.Max(0, WorkingDaysPerMonth - totalMarkedDays);
        var perDaySalary = employee.MonthlySalary / WorkingDaysPerMonth;
        var absentDeduction = Math.Round(perDaySalary * absentDays, 2);
        var netSalary = employee.MonthlySalary - absentDeduction - req.ManualDeduction;

        var payroll = new PayrollRecord
        {
            EmployeeId = req.EmployeeId,
            Month = req.Month,
            Year = req.Year,
            GrossSalary = employee.MonthlySalary,
            WorkingDays = WorkingDaysPerMonth,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            AbsentDeduction = absentDeduction,
            ManualDeduction = req.ManualDeduction,
            DeductionNote = req.DeductionNote,
            NetSalary = netSalary
        };

        db.PayrollRecords.Add(payroll);
        await db.SaveChangesAsync();

        return (true, "Payroll generated", ToResponse(payroll, employee.User.Name));
    }

    public async Task<PayrollResponse?> GetAsync(int employeeId, int month, int year)
    {
        var p = await db.PayrollRecords
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Month == month && p.Year == year);

        return p == null ? null : ToResponse(p, p.Employee.User.Name);
    }

    private static PayrollResponse ToResponse(PayrollRecord p, string name) => new(
        p.Id, p.EmployeeId, name, p.Month, p.Year,
        p.GrossSalary, p.WorkingDays, p.PresentDays, p.AbsentDays,
        p.AbsentDeduction, p.ManualDeduction, p.DeductionNote,
        p.NetSalary, p.Status
    );
}
