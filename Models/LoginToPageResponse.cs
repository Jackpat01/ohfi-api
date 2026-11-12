using MSLC.ArumProxy.Models;

namespace OhfiApi.Models;

public class LoginToPageResponse
{
    public bool Successful { get; set; } = false;
    public bool RedirectToTerm { get; set; } = false;
    public string ErrorMessage { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ARUMProxy_User? ARUMUser { get; set; } = null;
}

