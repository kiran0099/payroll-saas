namespace PayrollAPI.DTOs;

public record MarkAttendanceRequest(
    int EmployeeId,
    DateOnly Date,
    string Status,       // Present | Absent | HalfDay
    TimeOnly? CheckIn,
    TimeOnly? CheckOut,
    string? Note
);

public record AttendanceResponse(
    int Id,
    int EmployeeId,
    string EmployeeName,
    DateOnly Date,
    string Status,
    TimeOnly? CheckIn,
    TimeOnly? CheckOut,
    string? Note
);

public record MonthlyAttendanceQuery(int EmployeeId, int Month, int Year);
