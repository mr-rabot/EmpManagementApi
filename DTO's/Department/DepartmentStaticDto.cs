public class DepartmentStatisticsDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public decimal AverageSalary { get; set; }
    public int OnLeaveEmployees { get; set; }
}