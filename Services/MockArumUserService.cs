using OhfiApi.Models;

namespace OhfiApi.Services;

/// <summary>
/// Mock implementation of ARUM user service for development
/// TODO: Replace with actual ARUM proxy integration
/// </summary>
public class MockArumUserService : IArumUserService
{
    private readonly ILogger<MockArumUserService> _logger;

    public MockArumUserService(ILogger<MockArumUserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Mock implementation - validates against hardcoded test credentials
    /// In production, this would call the actual ARUM proxy service
    /// </summary>
    public async Task<LoginToPageResponse> GetUserRolesByEmailAndPasswordAsync(
        string email,
        string password,
        string operationName,
        string ipAddress)
    {
        await Task.Delay(100); // Simulate async call

        // Mock user database for testing
        var mockUsers = new Dictionary<(string, string), List<ARUMUser>>
        {
            {
                ("admin@example.com", "admin123"),
                new List<ARUMUser>
                {
                    new ARUMUser
                    {
                        UserID = 1,
                        Email = "admin@example.com",
                        AcceptedTerms = true,
                        Token = "ARUM_ADMIN_TOKEN",
                        IPAddress = ipAddress
                    }
                }
            },
            {
                ("user@example.com", "user123"),
                new List<ARUMUser>
                {
                    new ARUMUser
                    {
                        UserID = 2,
                        Email = "user@example.com",
                        AcceptedTerms = null, // User needs to accept terms
                        Token = "ARUM_USER_TOKEN",
                        IPAddress = ipAddress
                    }
                }
            },
            {
                ("multiclass@example.com", "pass123"),
                new List<ARUMUser>
                {
                    new ARUMUser
                    {
                        UserID = 3,
                        Email = "multiclass@example.com",
                        AcceptedTerms = true,
                        Token = "ARUM_TOKEN_CLASS1",
                        IPAddress = ipAddress
                    },
                    new ARUMUser
                    {
                        UserID = 4,
                        Email = "multiclass@example.com",
                        AcceptedTerms = null,
                        Token = "ARUM_TOKEN_CLASS2",
                        IPAddress = ipAddress
                    }
                }
            }
        };

        var key = (email, password);
        if (mockUsers.TryGetValue(key, out var users))
        {
            _logger.LogInformation("Mock ARUM authentication succeeded for: {Email}", email);
            return new LoginToPageResponse
            {
                Successful = true,
                Message = "Login successful",
                LstARUMUsers = users
            };
        }

        _logger.LogWarning("Mock ARUM authentication failed for: {Email}", email);
        return new LoginToPageResponse
        {
            Successful = false,
            Message = "Username/password invalid",
            LstARUMUsers = new List<ARUMUser>()
        };
    }

    /// <summary>
    /// Mock implementation for getting user roles by email only
    /// Used in AcceptTermsOfUse flow
    /// </summary>
    public async Task<LoginToPageResponse> GetUserRolesByEmailAsync(
        string email,
        string operationName,
        string ipAddress)
    {
        await Task.Delay(100); // Simulate async call

        // For this mock, just return a user if email matches
        if (email == "admin@example.com")
        {
            return new LoginToPageResponse
            {
                Successful = true,
                Message = "Users found",
                LstARUMUsers = new List<ARUMUser>
                {
                    new ARUMUser
                    {
                        UserID = 1,
                        Email = email,
                        AcceptedTerms = null,
                        Token = "ARUM_ADMIN_TOKEN",
                        IPAddress = ipAddress
                    }
                }
            };
        }

        return new LoginToPageResponse
        {
            Successful = false,
            Message = "No users found",
            LstARUMUsers = new List<ARUMUser>()
        };
    }

    /// <summary>
    /// Mock implementation - marks user terms as accepted
    /// </summary>
    public async Task<bool> UpdateAcceptedTermsAsync(int userID, string token)
    {
        await Task.Delay(50); // Simulate async call
        _logger.LogInformation("Mock: Updated accepted terms for user {UserID}", userID);
        return true;
    }

    /// <summary>
    /// Mock implementation - updates user's IP address
    /// </summary>
    public async Task<bool> UpdateUserIPAddressAsync(int userID, string ipAddress)
    {
        await Task.Delay(50); // Simulate async call
        _logger.LogInformation("Mock: Updated IP for user {UserID} to {IPAddress}", userID, ipAddress);
        return true;
    }
}
