using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.DTOs.Auth;

namespace EmployeeManagementSystem.Services;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RegisterAsync(RegisterRequest request);
    Task<User?> GetUserByIdAsync(int userId);
    Task LogoutAsync(int userId);
}