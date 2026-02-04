using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.DTOs.Auth;
using EmployeeManagementSystem.Repositories;
using EmployeeManagementSystem.Helpers;

namespace EmployeeManagementSystem.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        // Get user by username or email
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        
        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username/email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        // Generate JWT token
        var (accessToken, expiration) = GenerateJwtToken(user);
        
        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return new TokenResponse
        {
            AccessToken = accessToken,
            Expiration = expiration,
            TokenType = "Bearer",
            UserInfo = MapToUserInfo(user)
        };
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate
        if (await _userRepository.IsUsernameExistsAsync(request.Username))
            throw new InvalidOperationException("Username already exists");

        if (await _userRepository.IsEmailExistsAsync(request.Email))
            throw new InvalidOperationException("Email already exists");

        if (request.Password != request.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match");

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.Username,
            LastName = string.Empty,
            Role = request.Role,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        // Generate token
        var (accessToken, expiration) = GenerateJwtToken(user);

        return new TokenResponse
        {
            AccessToken = accessToken,
            Expiration = expiration,
            TokenType = "Bearer",
            UserInfo = MapToUserInfo(user)
        };
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    /// <summary>
    /// Logout user (client-side token removal)
    /// Server doesn't need to do anything for JWT logout
    /// </summary>
    public Task LogoutAsync(int userId)
    {
        // For JWT, logout is handled client-side by removing the token
        // No server-side action needed
        return Task.CompletedTask;
    }

    private (string token, DateTime expiration) GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        var expiration = token.ValidTo;

        return (jwtToken, expiration);
    }

    private UserInfo MapToUserInfo(User user)
    {
        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }

    public Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        throw new NotImplementedException();
    }
}