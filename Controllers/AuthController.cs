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
    private readonly IArumUserService _arumUserService;
    private readonly IIPAddressService _ipAddressService;

    public AuthController(
        IUserService userService,
        IJwtService jwtService,
        ILogger<AuthController> logger,
        IArumUserService arumUserService,
        IIPAddressService ipAddressService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
        _arumUserService = arumUserService;
        _ipAddressService = ipAddressService;
    }

    /// <summary>
    /// LoginToPage endpoint - ARUM-based authentication for legacy system
    /// Validates email/password and returns ARUM user validations + JWT token
    /// Supports multiple classifications/tokens per email
    /// </summary>
    [HttpPost("login-to-page")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginToPageResponse>> LoginToPage([FromBody] LoginRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("LoginToPage attempt with missing credentials");
                return BadRequest(new LoginToPageResponse
                {
                    Successful = false,
                    Message = "Email and password are required",
                    LstARUMUsers = new List<ARUMUser>()
                });
            }

            // Get user's external IP address
            var externalIpAddress = _ipAddressService.GetUserIPAddress(HttpContext);

            // Call ARUM service to validate credentials
            var arumValidations = await _arumUserService.GetUserRolesByEmailAndPasswordAsync(
                email: request.Email,
                password: request.Password,
                operationName: "LoginToPage",
                ipAddress: externalIpAddress
            );

            // Check if validation failed
            if (arumValidations == null || !arumValidations.Successful)
            {
                _logger.LogWarning("Failed LoginToPage attempt for email: {Email}", request.Email);
                return Ok(new LoginToPageResponse
                {
                    Successful = false,
                    Message = "Username/password invalid",
                    LstARUMUsers = new List<ARUMUser>()
                });
            }

            // Check if any ARUM users returned
            if (!arumValidations.LstARUMUsers.Any())
            {
                _logger.LogWarning("No ARUM users found for email: {Email}", request.Email);
                return Ok(new LoginToPageResponse
                {
                    Successful = false,
                    Message = "Username/password invalid",
                    LstARUMUsers = new List<ARUMUser>()
                });
            }

            // Process each ARUM user - update IP if changed
            var processedUsers = new List<ARUMUser>();
            foreach (var arumUser in arumValidations.LstARUMUsers)
            {
                // Update IP address if changed
                if (arumUser.IPAddress != externalIpAddress)
                {
                    var updateResult = await _arumUserService.UpdateUserIPAddressAsync(
                        arumUser.UserID,
                        externalIpAddress
                    );

                    if (!updateResult)
                    {
                        _logger.LogWarning("Failed to update IP for user {UserID}", arumUser.UserID);
                        // Don't fail the login, just log the warning
                    }
                }

                processedUsers.Add(new ARUMUser
                {
                    UserID = arumUser.UserID,
                    Email = arumUser.Email,
                    AcceptedTerms = arumUser.AcceptedTerms,
                    Token = arumUser.Token,
                    IPAddress = externalIpAddress
                });
            }

            // Generate JWT token for this authenticated user
            // Create a synthetic user object for JWT generation
            var jwtUser = new User
            {
                Id = processedUsers.First().UserID,
                Email = request.Email,
                Role = "User",
                Provider = "ARUM",
                Company = "OHFI",
                IsActive = true
            };

            var accessToken = _jwtService.GenerateAccessToken(jwtUser);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var token = new AuthToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                TokenType = "Bearer"
            };

            _logger.LogInformation("Successful LoginToPage for email: {Email} with {UserCount} ARUM users",
                request.Email, processedUsers.Count);

            return Ok(new LoginToPageResponse
            {
                Successful = true,
                Message = "Login successful",
                LstARUMUsers = processedUsers,
                Token = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoginToPage for email: {Email}", request.Email);
            return StatusCode(500, new LoginToPageResponse
            {
                Successful = false,
                Message = "An error occurred during login. Please try again.",
                LstARUMUsers = new List<ARUMUser>()
            });
        }
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
