namespace OhfiApi.Models;

/// <summary>
/// Represents an ARUM (MSLC Proxy) user validated during login
/// Supports multiple classifications/tokens for the same email
/// </summary>
public class ARUMUser
{
    public int UserID { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool? AcceptedTerms { get; set; }
    public string Token { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
}
