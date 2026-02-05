using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Helpers;

namespace EmployeeManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.RequireEmployee)]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Department>>> GetAll()
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return Ok(departments);
    }

    /// <summary>
    /// Get department by ID with employee count
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDetailDto>> GetById(int id)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == id && d.IsActive)
            .Select(d => new DepartmentDetailDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                ManagerId = d.ManagerId,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                EmployeeCount = d.Employees.Count(e => e.Status == EmploymentStatus.Active)
            })
            .FirstOrDefaultAsync();

        if (department == null)
            return NotFound(new { message = $"Department with ID {id} not found" });

        return Ok(department);
    }

    /// <summary>
    /// Create new department
    /// Requires Admin or HR role
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireHR)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Department>> Create([FromBody] CreateDepartmentDto dto)
    {
        var department = new Department
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            ManagerId = dto.ManagerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    /// <summary>
    /// Update department
    /// Requires Admin or HR role
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = PolicyNames.RequireHR)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Department>> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var department = await _context.Departments.FindAsync(id);
        
        if (department == null)
            return NotFound(new { message = $"Department with ID {id} not found" });

        if (!string.IsNullOrWhiteSpace(dto.Name))
            department.Name = dto.Name;
        
        if (!string.IsNullOrWhiteSpace(dto.Code))
            department.Code = dto.Code;
        
        if (!string.IsNullOrWhiteSpace(dto.Description))
            department.Description = dto.Description;
        
        if (dto.ManagerId.HasValue)
            department.ManagerId = dto.ManagerId.Value;
        
        if (dto.IsActive.HasValue)
            department.IsActive = dto.IsActive.Value;

        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(department);
    }

    /// <summary>
    /// Delete department
    /// Requires Admin role
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        
        if (department == null)
            return NotFound(new { message = $"Department with ID {id} not found" });

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get departments with statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentStatisticsDto>>> GetStatistics()
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .Include(d => d.Employees)
            .ToListAsync();

        var statistics = departments.Select(d => 
        {
            var employees = d.Employees
                .Where(e => e.Status != EmploymentStatus.Terminated)
                .ToList();

            return new DepartmentStatisticsDto
            {
                DepartmentId = d.Id,
                DepartmentName = d.Name,
                DepartmentCode = d.Code,
                TotalEmployees = employees.Count,
                ActiveEmployees = employees.Count(e => e.Status == EmploymentStatus.Active),
                OnLeaveEmployees = employees.Count(e => e.Status == EmploymentStatus.OnLeave),
                AverageSalary = employees.Any() 
                    ? employees.Average(e => e.Salary) 
                    : 0
            };
        }).ToList();

        return Ok(statistics);
    }
}





