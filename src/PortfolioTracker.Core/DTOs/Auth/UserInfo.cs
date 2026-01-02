namespace PortfolioTracker.Core.DTOs.Auth;

/// <summary>
/// User information included in authentication response.
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
