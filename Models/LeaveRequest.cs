namespace EmployeeManagementSystem.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    
    public string? ManagerComments { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum LeaveType
{
    Annual,
    Sick,
    Casual,
    Maternity,
    Paternity,
    Unpaid,
    Bereavement
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}