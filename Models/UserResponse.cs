namespace OhfiApi.Models;

public class UserResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; } = true;
}
