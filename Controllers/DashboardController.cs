using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OhfiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DashboardController : ControllerBase
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Example protected endpoint - requires valid JWT token
    /// Returns personalized dashboard data based on user claims
    /// 
    /// Old ASP.NET MVC pattern:
    /// [HttpGet]
    /// public ActionResult Dashboard()
    /// {
    ///   var userId = (int)Session["UserId"];
    ///   var userRole = (string)Session["UserRole"];
    ///   var company = (string)Session["Company"];
    ///   var data = GetDashboardData(userId, userRole);
    ///   return View(data);
    /// }
    /// 
    /// New ASP.NET WebAPI + JWT pattern:
    /// [HttpGet]
    /// [Authorize]
    /// public IActionResult Dashboard()
    /// {
    ///   var userId = User.FindFirst(ClaimTypes.NameIdentifier);
    ///   var userRole = User.FindFirst(ClaimTypes.Role);
    ///   var company = User.FindFirst("company");
    ///   var data = GetDashboardData(userId, userRole);
    ///   return Ok(data);
    /// }
    /// </summary>
    [HttpGet]
    public IActionResult GetDashboard()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var company = User.FindFirst("company")?.Value;
        var provider = User.FindFirst("provider")?.Value;

        _logger.LogInformation("Dashboard accessed by: {Email} ({UserId})", email, userId);

        var dashboardData = new
        {
            greeting = $"Welcome, {email}!",
            user = new
            {
                id = userId,
                email = email,
                role = role,
                company = company,
                provider = provider
            },
            stats = new
            {
                totalProjects = 5,
                activeUsers = 12,
                completedTasks = 42,
                pendingTasks = 8
            },
            recentActivity = new[]
            {
                new { date = DateTime.UtcNow.AddDays(-1), action = "Login", status = "Success" },
                new { date = DateTime.UtcNow.AddDays(-2), action = "Form Submission", status = "Success" },
                new { date = DateTime.UtcNow.AddDays(-3), action = "Profile Update", status = "Success" }
            }
        };

        return Ok(new
        {
            dashboardData = dashboardData,
            message = "Dashboard data loaded from protected API endpoint"
        });
    }

    /// <summary>
    /// Example: Admin-only endpoint
    /// Demonstrates role-based authorization from JWT claims
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetAdminDashboard()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        _logger.LogInformation("Admin dashboard accessed by: {Email}", email);

        return Ok(new
        {
            message = "Admin dashboard data",
            adminStats = new
            {
                totalUsers = 150,
                activeUsers = 87,
                inactiveUsers = 63,
                adminCount = 5
            }
        });
    }
}
