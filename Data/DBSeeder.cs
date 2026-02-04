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
            Role = UserRole.Admin,
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
            Role = UserRole.HR,
            PasswordHash = PasswordHasher.HashPassword("Hr@123"),
            IsActive = true
        };

        await context.Users.AddAsync(hrManagerUser);
        await context.SaveChangesAsync();

        // Seed IT Manager
        var itManagerUser = new User
        {
            Username = "itmanager",
            Email = "it@company.com",
            FirstName = "IT",
            LastName = "Manager",
            Role = UserRole.Manager,
            PasswordHash = PasswordHasher.HashPassword("It@123"),
            IsActive = true
        };

        await context.Users.AddAsync(itManagerUser);
        await context.SaveChangesAsync();

        // Seed Finance Manager
        var financeManagerUser = new User
        {
            Username = "financemanager",
            Email = "finance@company.com",
            FirstName = "Finance",
            LastName = "Manager",
            Role = UserRole.Manager,
            PasswordHash = PasswordHasher.HashPassword("Finance@123"),
            IsActive = true
        };

        await context.Users.AddAsync(financeManagerUser);
        await context.SaveChangesAsync();

        // Seed Employees (13 total - 3 existing + 10 new)
        var employees = new List<Employee>
        {
            // ========== Existing Employees ==========
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
            },

            // ========== NEW EMPLOYEES (10 more) ==========

            // IT Department - Senior Roles
            new()
            {
                EmployeeCode = "EMP004",
                FirstName = "Michael",
                LastName = "Chen",
                Email = "michael.chen@company.com",
                Phone = "+1234567893",
                DateOfBirth = new DateTime(1985, 11, 22),
                Gender = "Male",
                Address = "101 Tech Blvd",
                City = "San Francisco",
                State = "CA",
                ZipCode = "94105",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "IT").Id,
                Designation = "Lead Software Engineer",
                HireDate = new DateTime(2020, 2, 15),
                Salary = 95000,
                Status = EmploymentStatus.Active,
                UserId = itManagerUser.Id
            },
            new()
            {
                EmployeeCode = "EMP005",
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@company.com",
                Phone = "+1234567894",
                DateOfBirth = new DateTime(1991, 7, 8),
                Gender = "Female",
                Address = "202 Innovation Dr",
                City = "Boston",
                State = "MA",
                ZipCode = "02108",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "IT").Id,
                Designation = "Full Stack Developer",
                HireDate = new DateTime(2022, 4, 1),
                Salary = 78000,
                Status = EmploymentStatus.Active
            },
            new()
            {
                EmployeeCode = "EMP006",
                FirstName = "David",
                LastName = "Wilson",
                Email = "david.wilson@company.com",
                Phone = "+1234567895",
                DateOfBirth = new DateTime(1989, 12, 3),
                Gender = "Male",
                Address = "303 Code Lane",
                City = "Austin",
                State = "TX",
                ZipCode = "78701",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "IT").Id,
                Designation = "DevOps Engineer",
                HireDate = new DateTime(2021, 9, 15),
                Salary = 82000,
                Status = EmploymentStatus.Active
            },

            // Finance Department
            new()
            {
                EmployeeCode = "EMP007",
                FirstName = "Emily",
                LastName = "Davis",
                Email = "emily.davis@company.com",
                Phone = "+1234567896",
                DateOfBirth = new DateTime(1987, 4, 18),
                Gender = "Female",
                Address = "404 Finance Ave",
                City = "New York",
                State = "NY",
                ZipCode = "10005",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "FIN").Id,
                Designation = "Senior Financial Analyst",
                HireDate = new DateTime(2019, 8, 1),
                Salary = 88000,
                Status = EmploymentStatus.Active,
                UserId = financeManagerUser.Id
            },
            new()
            {
                EmployeeCode = "EMP008",
                FirstName = "Robert",
                LastName = "Brown",
                Email = "robert.brown@company.com",
                Phone = "+1234567897",
                DateOfBirth = new DateTime(1990, 9, 25),
                Gender = "Male",
                Address = "505 Accounting St",
                City = "Chicago",
                State = "IL",
                ZipCode = "60603",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "FIN").Id,
                Designation = "Accountant",
                HireDate = new DateTime(2022, 1, 10),
                Salary = 65000,
                Status = EmploymentStatus.Active
            },

            // Marketing Department
            new()
            {
                EmployeeCode = "EMP009",
                FirstName = "Lisa",
                LastName = "Martinez",
                Email = "lisa.martinez@company.com",
                Phone = "+1234567898",
                DateOfBirth = new DateTime(1993, 2, 14),
                Gender = "Female",
                Address = "606 Brand Blvd",
                City = "Los Angeles",
                State = "CA",
                ZipCode = "90024",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "MKT").Id,
                Designation = "Marketing Manager",
                HireDate = new DateTime(2020, 6, 15),
                Salary = 80000,
                Status = EmploymentStatus.Active
            },
            new()
            {
                EmployeeCode = "EMP010",
                FirstName = "James",
                LastName = "Taylor",
                Email = "james.taylor@company.com",
                Phone = "+1234567899",
                DateOfBirth = new DateTime(1994, 11, 30),
                Gender = "Male",
                Address = "707 Social Media Dr",
                City = "Miami",
                State = "FL",
                ZipCode = "33101",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "MKT").Id,
                Designation = "Digital Marketing Specialist",
                HireDate = new DateTime(2023, 3, 1),
                Salary = 62000,
                Status = EmploymentStatus.Active
            },

            // Sales Department
            new()
            {
                EmployeeCode = "EMP011",
                FirstName = "Jennifer",
                LastName = "Anderson",
                Email = "jennifer.anderson@company.com",
                Phone = "+1234567900",
                DateOfBirth = new DateTime(1988, 6, 12),
                Gender = "Female",
                Address = "808 Sales Ave",
                City = "Dallas",
                State = "TX",
                ZipCode = "75201",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "SLS").Id,
                Designation = "Sales Director",
                HireDate = new DateTime(2019, 4, 1),
                Salary = 92000,
                Status = EmploymentStatus.Active
            },
            new()
            {
                EmployeeCode = "EMP012",
                FirstName = "Thomas",
                LastName = "Garcia",
                Email = "thomas.garcia@company.com",
                Phone = "+1234567901",
                DateOfBirth = new DateTime(1992, 8, 17),
                Gender = "Male",
                Address = "909 Revenue Rd",
                City = "Atlanta",
                State = "GA",
                ZipCode = "30303",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "SLS").Id,
                Designation = "Sales Representative",
                HireDate = new DateTime(2022, 7, 15),
                Salary = 68000,
                Status = EmploymentStatus.Active
            },
            new()
            {
                EmployeeCode = "EMP013",
                FirstName = "Michelle",
                LastName = "Rodriguez",
                Email = "michelle.rodriguez@company.com",
                Phone = "+1234567902",
                DateOfBirth = new DateTime(1991, 3, 22),
                Gender = "Female",
                Address = "1010 Client St",
                City = "Seattle",
                State = "WA",
                ZipCode = "98101",
                Country = "USA",
                DepartmentId = departments.First(d => d.Code == "SLS").Id,
                Designation = "Business Development Manager",
                HireDate = new DateTime(2021, 5, 10),
                Salary = 76000,
                Status = EmploymentStatus.Active
            }
        };

        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Database seeded successfully!");
        Console.WriteLine($"   - {departments.Count} departments created");
        Console.WriteLine($"   - {employees.Count} employees created");
        Console.WriteLine($"   - 5 users created (Admin, HR Manager, IT Manager, Finance Manager, 3 employees)");
    }
}