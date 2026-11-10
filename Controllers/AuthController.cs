using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OhfiApi.Models;
using OhfiApi.Services;

namespace OhfiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint - returns JWT token and user info
    /// Replaces old ASP.NET session-based authentication
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login attempt with missing credentials");
            return BadRequest(new { message = "Email and password are required" });
        }

        var user = await _userService.AuthenticateAsync(request.Email, request.Password);
        if (user == null)
        {
            _logger.LogWarning("Login failed for email: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Generate JWT token with user claims
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var response = new LoginResponse
        {
            User = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                Provider = user.Provider,
                Company = user.Company,
                IsAuthenticated = true
            },
            Token = new AuthToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                TokenType = "Bearer"
            }
        };

        _logger.LogInformation("User logged in: {Email}", user.Email);
        return Ok(response);
    }

    /// <summary>
    /// Logout endpoint - in JWT pattern, logout happens client-side
    /// This endpoint can be used for cleanup tasks (invalidate refresh tokens, etc.)
    /// </summary>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult Logout()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        _logger.LogInformation("User logged out: {Email}", email);
        return Ok(new { message = "Logout successful" });
    }

    /// <summary>
    /// Get current user info - returns user data from JWT claims
    /// Replaces old Session["User"] access
    /// </summary>
    [HttpGet("user")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            Provider = user.Provider,
            Company = user.Company,
            IsAuthenticated = true
        });
    }
}
