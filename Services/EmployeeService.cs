using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.DTOs.Employee;
using EmployeeManagementSystem.DTOs.Common;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;

namespace EmployeeManagementSystem.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IGenericRepository<Department> _departmentRepository;

    public EmployeeService(
        AppDbContext context,
        IGenericRepository<Department> departmentRepository)
    {
        _context = context;
        _departmentRepository = departmentRepository;
    }

    public async Task<Pagination<EmployeeDto>> GetAllEmployeesAsync(EmployeeFilter filter, PaginationParams pagination)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.Status != EmploymentStatus.Terminated);

        query = ApplyFilters(query, filter);
        query = ApplySorting(query, filter.SortBy, filter.SortDescending);

        var totalCount = await query.CountAsync();

        var employees = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender,
                Address = e.Address,
                City = e.City,
                State = e.State,
                ZipCode = e.ZipCode,
                Country = e.Country,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.Name,
                Designation = e.Designation,
                HireDate = e.HireDate,
                Status = e.Status,
                Salary = e.Salary,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync();

        return new Pagination<EmployeeDto>
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            Items = employees
        };
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.Id == id && e.Status != EmploymentStatus.Terminated)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender,
                Address = e.Address,
                City = e.City,
                State = e.State,
                ZipCode = e.ZipCode,
                Country = e.Country,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.Name,
                Designation = e.Designation,
                HireDate = e.HireDate,
                Status = e.Status,
                Salary = e.Salary,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return employee;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, int? createdByUserId = null)
    {
        var departmentExists = await _context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Id == dto.DepartmentId && d.IsActive);

        if (!departmentExists)
            throw new InvalidOperationException("Department not found or inactive");

        var employeeCode = await GenerateEmployeeCodeAsync();

        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            DepartmentId = dto.DepartmentId,
            Designation = dto.Designation,
            HireDate = dto.HireDate,
            Salary = dto.Salary,
            Status = EmploymentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdByUserId?.ToString()
        };

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        var employeeDto = new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender,
            Address = employee.Address,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            Country = employee.Country,
            DepartmentId = employee.DepartmentId,
            DepartmentName = (await _context.Departments.FindAsync(employee.DepartmentId))?.Name ?? string.Empty,
            Designation = employee.Designation,
            HireDate = employee.HireDate,
            Status = employee.Status,
            Salary = employee.Salary,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };

        return employeeDto;
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, int? updatedByUserId = null)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id && e.Status != EmploymentStatus.Terminated);

        if (employee == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            employee.FirstName = dto.FirstName;
        
        if (!string.IsNullOrWhiteSpace(dto.LastName))
            employee.LastName = dto.LastName;
        
        if (!string.IsNullOrWhiteSpace(dto.Email))
            employee.Email = dto.Email;
        
        if (!string.IsNullOrWhiteSpace(dto.Phone))
            employee.Phone = dto.Phone;
        
        if (!string.IsNullOrWhiteSpace(dto.Address))
            employee.Address = dto.Address;
        
        if (dto.DateOfBirth.HasValue)
            employee.DateOfBirth = dto.DateOfBirth.Value;
        
        if (!string.IsNullOrWhiteSpace(dto.Gender))
            employee.Gender = dto.Gender;
        
        if (dto.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AsNoTracking()
                .AnyAsync(d => d.Id == dto.DepartmentId.Value && d.IsActive);
            
            if (departmentExists)
                employee.DepartmentId = dto.DepartmentId.Value;
        }
        
        if (!string.IsNullOrWhiteSpace(dto.Designation))
            employee.Designation = dto.Designation;
        
        if (dto.Salary.HasValue)
            employee.Salary = dto.Salary.Value;
        
        if (dto.Status.HasValue)
            employee.Status = dto.Status.Value;

        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = updatedByUserId?.ToString();

        await _context.SaveChangesAsync();

        var updatedEmployee = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender,
                Address = e.Address,
                City = e.City,
                State = e.State,
                ZipCode = e.ZipCode,
                Country = e.Country,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.Name,
                Designation = e.Designation,
                HireDate = e.HireDate,
                Status = e.Status,
                Salary = e.Salary,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return updatedEmployee;
    }

    public async Task<bool> SoftDeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id && e.Status != EmploymentStatus.Terminated);

        if (employee == null)
            return false;

        employee.Status = EmploymentStatus.Terminated;
        employee.TerminationDate = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        
        if (employee == null)
            return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        
        return true;
    }

   public async Task<decimal> GetTotalSalaryByDepartmentAsync(int departmentId)
{
    // ✅ SAFE: Minimal data transfer + preserves decimal precision
    var salaries = await _context.Employees
        .AsNoTracking()
        .Where(e => e.DepartmentId == departmentId && 
                   e.Status == EmploymentStatus.Active)
        .Select(e => e.Salary) // Projects ONLY salary values
        .ToListAsync(); // Executes query, brings values to client

    return salaries.Sum(); // LINQ-to-Objects handles sum safely
}

    public async Task<int> GetActiveEmployeeCountAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .CountAsync(e => e.Status == EmploymentStatus.Active);
    }

    public async Task<IEnumerable<EmployeeStatistics>> GetEmployeeStatisticsAsync()
{
    // ✅ Load ONLY required fields (minimal memory footprint)
    var employeeData = await _context.Employees
        .AsNoTracking()
        .Where(e => e.Status != EmploymentStatus.Terminated)
        .Select(e => new { e.DepartmentId, e.Salary, e.Status })
        .ToListAsync();

    // Get department names for referenced departments (mimics original INNER JOIN)
    var departmentIds = employeeData.Select(e => e.DepartmentId).Distinct().ToList();
    var departments = await _context.Departments
        .AsNoTracking()
        .Where(d => departmentIds.Contains(d.Id)) // NO IsActive filter (matches original JOIN behavior)
        .ToDictionaryAsync(d => d.Id, d => d.Name);

    // ✅ Client-side aggregation (preserves decimal precision for AverageSalary)
    return employeeData
        .GroupBy(e => e.DepartmentId)
        .Where(g => departments.ContainsKey(g.Key)) // Maintains INNER JOIN semantics
        .Select(g => 
        {
            var employees = g.ToList();
            return new EmployeeStatistics
            {
                DepartmentName = departments[g.Key],
                EmployeeCount = employees.Count,
                AverageSalary = employees.Average(e => e.Salary), // Safe in-memory calc
                ActiveCount = employees.Count(e => e.Status == EmploymentStatus.Active),
                OnLeaveCount = employees.Count(e => e.Status == EmploymentStatus.OnLeave)
            };
        })
        .ToList();
}

    public async Task<byte[]> ExportEmployeesToExcelAsync(EmployeeFilter filter)
    {
        // Inline filter logic - no extension method needed
        var query = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.Status != EmploymentStatus.Terminated);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(searchLower) ||
                e.LastName.ToLower().Contains(searchLower) ||
                e.Email.ToLower().Contains(searchLower) ||
                e.EmployeeCode.ToLower().Contains(searchLower) ||
                e.Designation.ToLower().Contains(searchLower));
        }

        if (filter.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);

        if (filter.Status.HasValue)
            query = query.Where(e => e.Status == filter.Status.Value);

        if (filter.HireDateFrom.HasValue)
            query = query.Where(e => e.HireDate >= filter.HireDateFrom.Value);

        if (filter.HireDateTo.HasValue)
            query = query.Where(e => e.HireDate <= filter.HireDateTo.Value);

        if (filter.MinSalary.HasValue)
            query = query.Where(e => e.Salary >= filter.MinSalary.Value);

        if (filter.MaxSalary.HasValue)
            query = query.Where(e => e.Salary <= filter.MaxSalary.Value);

        var employees = await query
            .Select(e => new
            {
                e.EmployeeCode,
                e.FirstName,
                e.LastName,
                e.Email,
                e.Phone,
                Department = e.Department.Name,
                e.Designation,
                e.HireDate,
                e.Salary,
                Status = e.Status.ToString()
            })
            .ToListAsync();

        return await Task.FromResult(new byte[0]);
    }

    #region Helper Methods

    private static IQueryable<Employee> ApplyFilters(IQueryable<Employee> query, EmployeeFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(searchLower) ||
                e.LastName.ToLower().Contains(searchLower) ||
                e.Email.ToLower().Contains(searchLower) ||
                e.EmployeeCode.ToLower().Contains(searchLower) ||
                e.Designation.ToLower().Contains(searchLower));
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(e => e.Status == filter.Status.Value);
        }

        if (filter.HireDateFrom.HasValue)
        {
            query = query.Where(e => e.HireDate >= filter.HireDateFrom.Value);
        }

        if (filter.HireDateTo.HasValue)
        {
            query = query.Where(e => e.HireDate <= filter.HireDateTo.Value);
        }

        if (filter.MinSalary.HasValue)
        {
            query = query.Where(e => e.Salary >= filter.MinSalary.Value);
        }

        if (filter.MaxSalary.HasValue)
        {
            query = query.Where(e => e.Salary <= filter.MaxSalary.Value);
        }

        return query;
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return descending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt);
        }

        return sortBy.ToLower() switch
        {
            "firstname" => descending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
            "lastname" => descending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
            "email" => descending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
            "salary" => descending ? query.OrderByDescending(e => e.Salary) : query.OrderBy(e => e.Salary),
            "hiredate" => descending ? query.OrderByDescending(e => e.HireDate) : query.OrderBy(e => e.HireDate),
            "department" => descending 
                ? query.OrderByDescending(e => e.DepartmentId) 
                : query.OrderBy(e => e.DepartmentId),
            "status" => descending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            _ => descending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt)
        };
    }

    private async Task<string> GenerateEmployeeCodeAsync()
    {
        var prefix = "EMP";
        var lastEmployee = await _context.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeCode.StartsWith(prefix))
            .OrderByDescending(e => e.EmployeeCode)
            .FirstOrDefaultAsync();

        if (lastEmployee == null)
            return $"{prefix}001";

        var lastCode = lastEmployee.EmployeeCode.Replace(prefix, "");
        if (int.TryParse(lastCode, out int lastNumber))
        {
            return $"{prefix}{(lastNumber + 1):D3}";
        }

        return $"{prefix}001";
    }

    #endregion
}