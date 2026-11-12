namespace OhfiApi.Services;

/// <summary>
/// Service for extracting user's IP address from various sources
/// Handles forwarded headers, proxy configurations, etc.
/// </summary>
public class IPAddressService : IIPAddressService
{
    private readonly ILogger<IPAddressService> _logger;

    public IPAddressService(ILogger<IPAddressService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts user's external IP address from HttpContext
    /// Checks X-Forwarded-For header first (for proxies), then remote address
    /// </summary>
    public string GetUserIPAddress(HttpContext context)
    {
        try
        {
            // Check for X-Forwarded-For header (common in proxy/load balancer scenarios)
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ips = forwardedFor.ToString().Split(',');
                if (ips.Length > 0 && !string.IsNullOrWhiteSpace(ips[0]))
                {
                    return ips[0].Trim();
                }
            }

            // Check for X-Real-IP header
            if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                if (!string.IsNullOrWhiteSpace(realIp))
                {
                    return realIp.ToString().Trim();
                }
            }

            // Fall back to RemoteIpAddress
            var remoteIp = context.Connection?.RemoteIpAddress?.ToString();
            return !string.IsNullOrWhiteSpace(remoteIp) ? remoteIp : "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting IP address");
            return "Unknown";
        }
    }
}
