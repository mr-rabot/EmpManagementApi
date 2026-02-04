public class LeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int AnnualLeaveBalance { get; set; }
    public int SickLeaveBalance { get; set; }
    public int CasualLeaveBalance { get; set; }
}