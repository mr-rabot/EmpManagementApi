namespace EmployeeManagementSystem.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee; // et default in C# instead of DB
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
    
    public Employee? Employee { get; set; }
}

// REORDER ENUM SO CLR DEFAULT = DESIRED DEFAULT
public enum UserRole
{
    Employee = 0,  // Must be first (CLR default = 0)
    Admin = 1,
    Manager = 2,
    HR = 3
}