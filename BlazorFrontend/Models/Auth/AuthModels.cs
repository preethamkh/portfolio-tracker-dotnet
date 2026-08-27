namespace BlazorFrontend.Models.Auth;

/// <summary>
/// Authenticated user information returned by the API.
/// Mirrors the backend's UserInfo DTO.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User's unique identifier. Used in API routes (/api/users/{userId}/...).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address (also used as username).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name.
    /// </summary>
    public string? FullName { get; set; }
}

/// <summary>
/// Response returned after successful login or registration.
/// Mirrors the backend's AuthResponse DTO.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT token. Sent as 'Bearer {token}' on every API call.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Authenticated user details.
    /// </summary>
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// Login request payload.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Registration request payload. Password rules are enforced server-side
/// (min 8 chars, upper/lower/digit/special) and mirrored client-side.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
