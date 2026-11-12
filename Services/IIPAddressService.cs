namespace OhfiApi.Services;

/// <summary>
/// Interface for IP address extraction and management
/// </summary>
public interface IIPAddressService
{
    /// <summary>
    /// Extracts user's external IP address from HttpContext
    /// Handles proxy headers and various network configurations
    /// </summary>
    string GetUserIPAddress(HttpContext context);
}
