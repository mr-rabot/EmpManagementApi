using EmployeeManagementSystem.DTOs.Employee;
using EmployeeManagementSystem.DTOs.Common;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services;

public interface IEmployeeService
{
    Task<Pagination<EmployeeDto>> GetAllEmployeesAsync(EmployeeFilter filter, PaginationParams pagination);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, int? createdByUserId = null);
    Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, int? updatedByUserId = null);
    Task<bool> DeleteEmployeeAsync(int id);
    Task<bool> SoftDeleteEmployeeAsync(int id);
    Task<decimal> GetTotalSalaryByDepartmentAsync(int departmentId);
    Task<int> GetActiveEmployeeCountAsync();
    Task<IEnumerable<EmployeeStatistics>> GetEmployeeStatisticsAsync();
    Task<byte[]> ExportEmployeesToExcelAsync(EmployeeFilter filter);
}

public class EmployeeFilter
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public EmploymentStatus? Status { get; set; }
    public DateTime? HireDateFrom { get; set; }
    public DateTime? HireDateTo { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class EmployeeStatistics
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AverageSalary { get; set; }
    public int ActiveCount { get; set; }
    public int OnLeaveCount { get; set; }
}