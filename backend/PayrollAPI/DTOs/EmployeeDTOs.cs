namespace PayrollAPI.DTOs;

public record CreateEmployeeRequest(
    string Name,
    string Email,
    string Password,
    string Department,
    string Designation,
    decimal MonthlySalary,
    DateOnly JoiningDate
);

public record UpdateEmployeeRequest(
    string Name,
    string Department,
    string Designation,
    decimal MonthlySalary,
    bool IsActive
);

public record EmployeeResponse(
    int Id,
    int UserId,
    string Name,
    string Email,
    string Department,
    string Designation,
    decimal MonthlySalary,
    DateOnly JoiningDate,
    bool IsActive
);
