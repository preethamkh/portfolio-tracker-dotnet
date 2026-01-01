using System.Security.Claims;
using PortfolioTracker.Core.Entities;

namespace PortfolioTracker.Core.Interfaces.Services;

/// <summary>
/// Service interface for JWT token management operations.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and returns the claims if valid.
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);
}
