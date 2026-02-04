namespace EmployeeManagementSystem.Models;

public class Payroll
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Bonuses { get; set; }
    public decimal Overtime { get; set; }
    public decimal Deductions { get; set; }
    public decimal Tax { get; set; }
    public decimal NetSalary { get; set; }
    
    public PaymentStatus Status { get; set; } = PaymentStatus.Processed; // Default in C#
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// EORDERED: CLR default (0) = Processed (desired default)
public enum PaymentStatus
{
    Processed = 0,  // Must be first - CLR default value
    Pending = 1,
    Paid = 2,
    Failed = 3
}