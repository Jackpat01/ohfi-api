using System.Net;

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
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp == null)
            {
                return string.Empty;
            }

            // Convert IPv6 loopback (::1) to IPv4 (127.0.0.1)
            if (IPAddress.IsLoopback(remoteIp))
            {
                return "127.0.0.1";
            }

            // Map IPv4-mapped IPv6 addresses (e.g., ::ffff:192.168.1.1) back to standard IPv4
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }

            return remoteIp.ToString();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting IP address");
            return "Unknown";
        }
    }
}
