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
    private readonly IArumService _arumUserService;
    private readonly IIPAddressService _ipAddressService;

    public AuthController(
        IUserService userService,
        IJwtService jwtService,
        ILogger<AuthController> logger,
        IArumService arumUserService,
        IIPAddressService ipAddressService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
        _arumUserService = arumUserService;
        _ipAddressService = ipAddressService;
    }

    [HttpPost("loginToPage")]
    [AllowAnonymous]
    public LoginToPageResponse LoginToPage([FromBody] LoginRequest request)
    {
        try
        {
            var externalIpAddress = _ipAddressService.GetUserIPAddress(HttpContext);
            return _arumUserService.GetUserRolesByEmailAndPassword(
                email: request.Email,
                password: request.Password,
                operationName: "LoginToPage",
                ipAddress: externalIpAddress
            );
        }
        catch (Exception ex)
        {
            var exMessage = ex.InnerException != null ? $"{ex.Message} Inner Exception: {ex.InnerException.Message}" : ex.Message;
            _logger.LogError(exMessage);
            return  new LoginToPageResponse();
        }
    }
}
