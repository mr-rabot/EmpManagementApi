using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.DTOs.Auth;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmployeeManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "usernameOrEmail": "admin",
    ///         "password": "Admin@123"
    ///     }
    /// </remarks>
    /// <response code="200">Returns JWT token and user info</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var tokenResponse = await _authService.LoginAsync(request);
            return Ok(tokenResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///         "username": "newuser",
    ///         "email": "newuser@example.com",
    ///         "password": "Password@123",
    ///         "confirmPassword": "Password@123",
    ///         "role": "Employee"
    ///     }
    /// </remarks>
    /// <response code="201">User created and token returned</response>
    /// <response code="400">Validation errors</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var tokenResponse = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Login), tokenResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout (client removes token)
    /// </summary>
    /// <remarks>
    /// For JWT authentication, logout is handled client-side by removing the token.
    /// This endpoint is provided for consistency but doesn't perform any server-side action.
    /// </remarks>
    /// <response code="200">Logout successful</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _authService.LogoutAsync(userId);
        
        return Ok(new { message = "Logout successful. Please remove token from client storage." });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <response code="200">Returns current user info</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfo>> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _authService.GetUserByIdAsync(userId);
        
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        });
    }
}