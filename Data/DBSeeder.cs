using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Helpers;

namespace EmployeeManagementSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Don't seed if data already exists
        if (await context.Departments.AnyAsync() || await context.Users.AnyAsync())
            return;

        // Seed Departments
        var departments = new List<Department>
        {
            new() { Name = "Human Resources", Code = "HR", Description = "HR Department" },
            new() { Name = "Information Technology", Code = "IT", Description = "IT Department" },
            new() { Name = "Finance", Code = "FIN", Description = "Finance Department" },
            new() { Name = "Marketing", Code = "MKT", Description = "Marketing Department" },
            new() { Name = "Sales", Code = "SLS", Description = "Sales Department" }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();

        // Seed Admin User (Password: Admin@123)
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@company.com",
            FirstName = "System",
            LastName = "Administrator",
            Role = UserRole.Admin, // ✅ Using enum name (value = 1)
            PasswordHash = PasswordHasher.HashPassword("Admin@123"),
            IsActive = true
        };

        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        // Seed HR Manager
        var hrManagerUser = new User
        {
            Username = "hrmanager",
            Email = "hr@company.com",
            FirstName = "HR",
            LastName = "Manager",
            Role = UserRole.HR, // ✅ Using enum name (value = 3)
            PasswordHash = PasswordHasher.HashPassword("Hr@123"),
            IsActive = true
        };

        await context.Users.AddAsync(hrManagerUser);
        await context.SaveChangesAsync();

        // Seed Employees
        var employees = new List<Employee>
        {
            new()
            {
                EmployeeCode = "EMP001",
                FirstName = "Raman",
                LastName = "Rawat",
                Email = "raman.rawat@company.com",
                Phone = "+1234567890",
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = "Male",
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "IT").Id,
                Designation = "Senior Developer",
                HireDate = new DateTime(2023, 1, 15),
                Salary = 85000,
                Status = EmploymentStatus.Active,
                UserId = adminUser.Id
            },
            new()
            {
                EmployeeCode = "EMP002",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                Phone = "+1234567891",
                DateOfBirth = new DateTime(1988, 8, 20),
                Gender = "Male",
                Address = "456 Oak Ave",
                City = "Los Angeles",
                State = "CA",
                ZipCode = "90001",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "HR").Id,
                Designation = "HR Manager",
                HireDate = new DateTime(2022, 6, 1),
                Salary = 75000,
                Status = EmploymentStatus.Active,
                UserId = hrManagerUser.Id
            },
            new()
            {
                EmployeeCode = "EMP003",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                Phone = "+1234567892",
                DateOfBirth = new DateTime(1992, 3, 10),
                Gender = "Female",
                Address = "789 Pine Rd",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "IT").Id,
                Designation = "Software Engineer",
                HireDate = new DateTime(2021, 3, 10),
                Salary = 70000,
                Status = EmploymentStatus.Active
            }
        };

        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Database seeded successfully!");
    }
}