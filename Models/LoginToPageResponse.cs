namespace OhfiApi.Models;

/// <summary>
/// Response model for LoginToPage endpoint
/// Returns ARUM user validations for authentication/authorization
/// </summary>
public class LoginToPageResponse
{
    public bool Successful { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ARUMUser> LstARUMUsers { get; set; } = new();
}
