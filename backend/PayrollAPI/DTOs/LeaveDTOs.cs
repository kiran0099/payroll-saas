namespace PayrollAPI.DTOs;

public record ApplyLeaveRequest(
    int EmployeeId,
    string LeaveType,   // Sick | Casual
    DateOnly FromDate,
    DateOnly ToDate,
    string Reason
);

public record ApproveLeaveRequest(
    int LeaveRequestId,
    string Status,      // Approved | Rejected
    string? AdminNote
);

public record LeaveResponse(
    int Id,
    int EmployeeId,
    string EmployeeName,
    string LeaveType,
    DateOnly FromDate,
    DateOnly ToDate,
    string Reason,
    string Status,
    string? AdminNote,
    DateTime CreatedAt
);
