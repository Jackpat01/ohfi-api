using OhfiApi.Models;

namespace OhfiApi.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int id);
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// In-memory user database for demo purposes.
    /// In production, use a real database (EF Core, Dapper, etc.)
    /// </summary>
    private static readonly Dictionary<string, User> Users = new()
    {
        {
            "admin@example.com",
            new User
            {
                Id = 1,
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Administrator",
                Provider = "Admin",
                Company = "OHFI",
                IsActive = true
            }
        },
        {
            "user@example.com",
            new User
            {
                Id = 2,
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "User",
                Provider = "Standard",
                Company = "OHFI",
                IsActive = true
            }
        },
        {
            "manager@example.com",
            new User
            {
                Id = 3,
                Email = "manager@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                Role = "Manager",
                Provider = "Standard",
                Company = "OHFI",
                IsActive = true
            }
        }
    };

    public Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = Users.Values.FirstOrDefault(u => u.Email == email && u.IsActive);
        if (user == null)
        {
            _logger.LogWarning("Authentication failed for email: {Email}", email);
            return Task.FromResult<User?>(null);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for email: {Email}", email);
            return Task.FromResult<User?>(null);
        }

        _logger.LogInformation("User authenticated: {Email}", email);
        return Task.FromResult<User?>(user);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        var user = Users.Values.FirstOrDefault(u => u.Email == email && u.IsActive);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        var user = Users.Values.FirstOrDefault(u => u.Id == id && u.IsActive);
        return Task.FromResult(user);
    }
}
