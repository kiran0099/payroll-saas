namespace PayrollAPI.DTOs;

public record GeneratePayrollRequest(
    int EmployeeId,
    int Month,
    int Year,
    decimal ManualDeduction,
    string? DeductionNote
);

public record PayrollResponse(
    int Id,
    int EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal GrossSalary,
    int WorkingDays,
    int PresentDays,
    int AbsentDays,
    decimal AbsentDeduction,
    decimal ManualDeduction,
    string? DeductionNote,
    decimal NetSalary,
    string Status
);
