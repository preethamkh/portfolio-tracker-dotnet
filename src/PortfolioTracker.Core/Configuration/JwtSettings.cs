namespace PortfolioTracker.Core.Configuration;

/// <summary>
/// Configuration settings for JWT (JSON Web Token) authentication.
/// These values are typically loaded from appsettings.json or environment variables.
/// </summary>
/// <remarks>
/// Security Note:
/// - Secret MUST be at least 32 characters for HS256 algorithm
/// - Never commit real secrets to git (use environment variables in production)
/// - Rotate secrets regularly in production
/// </remarks>
public class JwtSettings
{
    /// <summary>
    /// Secret key for signing tokens.
    /// MUST be at least 32 characters for HS256 algorithm.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer - who created and signed the token.
    /// Usually the auth server or API name.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience - who the token is intended for.
    /// Usually the client app name or domain.
    /// </summary>
    /// <remarks>
    /// "PortfolioTrackerWeb" or "https://portfolio-tracker-railway.app".
    /// </remarks>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiry time in minutes.
    /// After this time, token becomes invalid and user must login again.
    /// </summary>
    // Default: 1 hour, but setting it to 30 minutes
    public int ExpirationInMinutes { get; set; } = 30;

}