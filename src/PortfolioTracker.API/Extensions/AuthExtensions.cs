using System.Security.Claims;

namespace PortfolioTracker.API.Extensions;

/// <summary>
/// Extension methods for working with authenticated users.
/// Provides helper methods to extract user information from JWT claims.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Gets the authenticated user's ID from their claims.
    /// </summary>
    /// <returns>User ID if authenticated, null otherwise</returns>
    public static Guid? GetAuthenticatedUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Checks if the authenticated user is authorized to access resources for the requested user ID.
    /// </summary>
    /// <param name="user">Claims principal from JWT</param>
    /// <param name="requestedUserId">User ID being accessed in the request</param>
    /// <returns>True if authorized, false otherwise</returns>
    public static bool IsAuthorizedForUser(this ClaimsPrincipal user, Guid requestedUserId)
    {
        var authenticatedUserId = user.GetAuthenticatedUserId();

        return authenticatedUserId.HasValue && authenticatedUserId.Value == requestedUserId;
    }
}
