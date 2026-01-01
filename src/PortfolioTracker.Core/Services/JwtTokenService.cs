using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PortfolioTracker.Core.Configuration;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.Core.Services;

public class JwtTokenService(JwtSettings jwtSettings) : IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate token for</param>
    /// <returns>JWT token string</returns>
    /// <remarks>
    /// Token Generation Process:
    /// 1. Create claims (user info)
    /// 2. Create signing credentials (secret key)
    /// 3. Create token descriptor (claims + credentials + expiry)
    /// 4. Generate token
    /// 5. Return as string
    /// </remarks>
    public string GenerateToken(User user)
    {
        // Step 1: Create claims (user info embedded in token)
        var claims = new List<Claim>
        {
            // Subject claim: User ID (standard JWT claim)
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            // Email claim: User's email address
            new Claim(JwtRegisteredClaimNames.Email, user.Email),

            // Name claim: (standard JWT claim)
            new Claim(JwtRegisteredClaimNames.Name, user.FullName ?? user.Email),

            // JWT ID: Unique identifier for this token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // Custom claim: User ID as NameIdentifier (ASP.NET Core convention)
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

            // Custom claim: Email as Name (ASP.NET Core convention)
            new Claim(ClaimTypes.Name, user.Email)
        };

        // Step 2: Create signing credentials using the secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));

        // Create credentials with HMAC SHA-256 algorithm
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Step 3: Create token descriptor with claims, credentials, and expiry
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience
        };

        // Step 4: Generate the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // Step 5: Return the token as a string
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims if valid.
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
    /// <remarks>
    /// Validation Process:
    /// 1. Parse token string
    /// 2. Verify signature with secret key
    /// 3. Check expiration time
    /// 4. Verify issuer and audience
    /// 5. Return claims if all checks pass
    /// </remarks>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

            // Validation parameters
            var validationParameters = new TokenValidationParameters
            {
                // Signature validation
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                // Issuer validation
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                // Audience validation
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                // Expiration validation
                ValidateLifetime = true,

                // Clock skew (allows for slight time differences between servers)
                ClockSkew = TimeSpan.Zero // Strict: no tolerance for clock differences
            };

            // Validate and return claims
            // discard validated token as it's not needed, we only need the principal
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            // Token is invalid (signature mismatch, expired, etc.)
            return null;
        }
    }
}
