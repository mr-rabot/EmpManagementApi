using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.DTOs.Employee;

public class UpdateEmployeeDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    
    public int? DepartmentId { get; set; }
    public string? Designation { get; set; }
    public decimal? Salary { get; set; }
    public EmploymentStatus? Status { get; set; }
}