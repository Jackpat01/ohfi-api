namespace OhfiApi.Models;

public class LoginResponse
{
    public UserResponse User { get; set; } = new();
    public AuthToken Token { get; set; } = new();
}
