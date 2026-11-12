using OhfiApi.Models;

namespace OhfiApi.Services;

/// <summary>
/// Interface for ARUM (MSLC Proxy) user validation and management
/// Handles authentication against legacy ARUM system
/// </summary>
public interface IArumUserService
{
    /// <summary>
    /// Validates user email and password against ARUM system
    /// Returns list of users if authentication succeeds
    /// </summary>
    Task<LoginToPageResponse> GetUserRolesByEmailAndPasswordAsync(
        string email,
        string password,
        string operationName,
        string ipAddress
    );

    /// <summary>
    /// Gets user roles by email without password validation
    /// Used for accepting terms workflow
    /// </summary>
    Task<LoginToPageResponse> GetUserRolesByEmailAsync(
        string email,
        string operationName,
        string ipAddress
    );

    /// <summary>
    /// Updates user's accepted terms status
    /// </summary>
    Task<bool> UpdateAcceptedTermsAsync(int userID, string token);

    /// <summary>
    /// Updates user's IP address for security tracking
    /// </summary>
    Task<bool> UpdateUserIPAddressAsync(int userID, string ipAddress);
}
