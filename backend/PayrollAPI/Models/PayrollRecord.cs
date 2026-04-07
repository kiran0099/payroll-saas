namespace PayrollAPI.Models;

public class PayrollRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }   // 1–12
    public int Year { get; set; }
    public decimal GrossSalary { get; set; }
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public decimal AbsentDeduction { get; set; }
    public decimal ManualDeduction { get; set; }
    public string? DeductionNote { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = "Generated"; // Generated | Paid
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
}
