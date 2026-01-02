namespace PortfolioTracker.Core.DTOs.Auth;

/// <summary>
/// Response DTO returned after successful authentication.
/// Contains JWT token and user information.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT token for authentication.
    /// Include this in Authorization header: 'Bearer {token}'
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// User information.
    /// </summary>
    public UserInfo User { get; set; } = new();
}
