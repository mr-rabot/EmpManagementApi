using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Helpers;
using System.Security.Claims;
using EmployeeManagementSystem.DTOs.Common;

namespace EmployeeManagementSystem.Controllers;

/// <summary>
/// Leave management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.RequireEmployee)]
[Produces("application/json")]
public class LeaveController : ControllerBase
{
    private readonly AppDbContext _context;

    public LeaveController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all leave requests with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireHR)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Pagination<LeaveRequestDto>>> GetAll(
        [FromQuery] int? employeeId = null,
        [FromQuery] LeaveType? type = null,
        [FromQuery] LeaveStatus? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        IQueryable<LeaveRequest> query = _context.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Employee)
            .ThenInclude(e => e.Department);

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        
        if (type.HasValue)
            query = query.Where(l => l.Type == type.Value);
        
        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);
        
        if (startDate.HasValue)
            query = query.Where(l => l.StartDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(l => l.EndDate <= endDate.Value);

        var totalCount = await query.CountAsync();

        var leaveRequests = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
                EmployeeCode = l.Employee.EmployeeCode,
                DepartmentName = l.Employee.Department.Name,
                Type = l.Type,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Days = l.Days,
                Reason = l.Reason,
                Status = l.Status,
                ManagerComments = l.ManagerComments,
                ApprovedBy = l.ApprovedBy,
                ApprovedAt = l.ApprovedAt,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        return Ok(new Pagination<LeaveRequestDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = leaveRequests
        });
    }

    /// <summary>
    /// Get current user's leave requests
    /// </summary>
    [HttpGet("my-leaves")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetMyLeaves()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
            return NotFound(new { message = "Employee profile not found" });

        var leaves = await _context.LeaveRequests
            .AsNoTracking()
            .Where(l => l.EmployeeId == employee.Id)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                Type = l.Type,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Days = l.Days,
                Reason = l.Reason,
                Status = l.Status,
                ManagerComments = l.ManagerComments,
                ApprovedBy = l.ApprovedBy,
                ApprovedAt = l.ApprovedAt,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(leaves);
    }

    /// <summary>
    /// Get leave request by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaveRequestDto>> GetById(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Employee)
            .ThenInclude(e => e.Department)
            .Where(l => l.Id == id)
            .Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
                EmployeeCode = l.Employee.EmployeeCode,
                DepartmentName = l.Employee.Department.Name,
                Type = l.Type,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Days = l.Days,
                Reason = l.Reason,
                Status = l.Status,
                ManagerComments = l.ManagerComments,
                ApprovedBy = l.ApprovedBy,
                ApprovedAt = l.ApprovedAt,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (leaveRequest == null)
            return NotFound(new { message = $"Leave request with ID {id} not found" });

        return Ok(leaveRequest);
    }

    /// <summary>
    /// Create new leave request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeaveRequestDto>> Create([FromBody] CreateLeaveRequestDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
            return NotFound(new { message = "Employee profile not found" });

        if (dto.StartDate > dto.EndDate)
            return BadRequest(new { message = "Start date cannot be after end date" });

        var days = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            Type = dto.Type,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Days = days,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = leaveRequest.Id }, new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            Type = leaveRequest.Type,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status,
            CreatedAt = leaveRequest.CreatedAt
        });
    }

    /// <summary>
    /// Approve or reject leave request
    /// </summary>
    [HttpPut("{id}/approve")]
    [Authorize(Policy = PolicyNames.RequireHR)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaveRequestDto>> Approve(int id, [FromBody] ApproveLeaveDto dto)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leaveRequest == null)
            return NotFound(new { message = $"Leave request with ID {id} not found" });

        leaveRequest.Status = dto.Approve ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leaveRequest.ManagerComments = dto.Comments;
        leaveRequest.ApprovedBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            Type = leaveRequest.Type,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status,
            ManagerComments = leaveRequest.ManagerComments,
            ApprovedBy = leaveRequest.ApprovedBy,
            ApprovedAt = leaveRequest.ApprovedAt,
            UpdatedAt = leaveRequest.UpdatedAt
        });
    }

    /// <summary>
    /// Cancel leave request
    /// </summary>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var leaveRequest = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leaveRequest == null)
            return NotFound(new { message = $"Leave request with ID {id} not found" });

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdminOrHR = userRole == UserRole.Admin.ToString() || userRole == UserRole.HR.ToString();
        
        if (!isAdminOrHR && leaveRequest.Employee.UserId != userId)
            return Forbid();

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(new { message = "Only pending leave requests can be cancelled" });

        leaveRequest.Status = LeaveStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Leave request cancelled successfully" });
    }

    /// <summary>
    /// Get leave balance for current employee
    /// </summary>
    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LeaveBalanceDto>> GetLeaveBalance()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
            return NotFound(new { message = "Employee profile not found" });

        var usedLeaves = await _context.LeaveRequests
            .AsNoTracking()
            .Where(l => l.EmployeeId == employee.Id && 
                       l.Status == LeaveStatus.Approved &&
                       l.StartDate.Year == DateTime.UtcNow.Year)
            .GroupBy(l => l.Type)
            .Select(g => new { Type = g.Key, Days = g.Sum(l => l.Days) })
            .ToListAsync();

        var balance = new LeaveBalanceDto
        {
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Year = DateTime.UtcNow.Year,
            AnnualLeaveBalance = 15 - (usedLeaves.FirstOrDefault(l => l.Type == LeaveType.Annual)?.Days ?? 0),
            SickLeaveBalance = 10 - (usedLeaves.FirstOrDefault(l => l.Type == LeaveType.Sick)?.Days ?? 0),
            CasualLeaveBalance = 7 - (usedLeaves.FirstOrDefault(l => l.Type == LeaveType.Casual)?.Days ?? 0)
        };

        return Ok(balance);
    }
}
