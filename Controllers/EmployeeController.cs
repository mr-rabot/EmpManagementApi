using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.DTOs.Employee;
using EmployeeManagementSystem.DTOs.Common;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using System.Security.Claims;

namespace EmployeeManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.RequireEmployee)]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Get all employees with filtering, sorting, and pagination
    /// Optimized for performance with minimal data transfer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Pagination<EmployeeDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] EmploymentStatus? status = null,
        [FromQuery] DateTime? hireDateFrom = null,
        [FromQuery] DateTime? hireDateTo = null,
        [FromQuery] decimal? minSalary = null,
        [FromQuery] decimal? maxSalary = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new EmployeeFilter
        {
            Search = search,
            DepartmentId = departmentId,
            Status = status,
            HireDateFrom = hireDateFrom,
            HireDateTo = hireDateTo,
            MinSalary = minSalary,
            MaxSalary = maxSalary,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _employeeService.GetAllEmployeesAsync(filter, paginationParams);
        return Ok(result);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        
        if (employee == null)
            return NotFound(new { message = $"Employee with ID {id} not found" });

        return Ok(employee);
    }

    /// <summary>
    /// Create new employee
    /// Requires HR or Admin role
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireHR)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var createdEmployee = await _employeeService.CreateEmployeeAsync(dto, userId);
        
        return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
    }

    /// <summary>
    /// Update employee
    /// Requires HR or Admin role
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = PolicyNames.RequireHR)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, dto, userId);
        
        if (updatedEmployee == null)
            return NotFound(new { message = $"Employee with ID {id} not found" });

        return Ok(updatedEmployee);
    }

    /// <summary>
    /// Soft delete employee (mark as terminated)
    /// Requires HR or Admin role
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = PolicyNames.RequireHR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var deleted = await _employeeService.SoftDeleteEmployeeAsync(id);
        
        if (!deleted)
            return NotFound(new { message = $"Employee with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Get total salary by department
    /// </summary>
    [HttpGet("department/{departmentId}/total-salary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> GetTotalSalaryByDepartment(int departmentId)
    {
        var totalSalary = await _employeeService.GetTotalSalaryByDepartmentAsync(departmentId);
        return Ok(new { departmentId, totalSalary });
    }

    /// <summary>
    /// Get active employee count
    /// </summary>
    [HttpGet("statistics/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetActiveEmployeeCount()
    {
        var count = await _employeeService.GetActiveEmployeeCountAsync();
        return Ok(new { activeEmployeeCount = count });
    }

    /// <summary>
    /// Get comprehensive employee statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeStatistics>>> GetStatistics()
    {
        var statistics = await _employeeService.GetEmployeeStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Export employees to Excel
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] string? search = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] EmploymentStatus? status = null)
    {
        var filter = new EmployeeFilter
        {
            Search = search,
            DepartmentId = departmentId,
            Status = status
        };

        var excelBytes = await _employeeService.ExportEmployeesToExcelAsync(filter);
        
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "employees.xlsx");
    }

    /// <summary>
    /// Get current user's employee profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var employee = await _employeeService.GetEmployeeByIdAsync(userId);
        
        if (employee == null)
            return NotFound(new { message = "Employee profile not found" });

        return Ok(employee);
    }
}