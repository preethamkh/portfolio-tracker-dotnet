namespace BlazorFrontend.Models;

/// <summary>
/// Portfolio summary as returned by the API.
/// Mirrors the backend's PortfolioDto (note: GUID identifiers).
/// </summary>
public class Portfolio
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = "AUD";
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int HoldingsCount { get; set; }
}

/// <summary>
/// Payload for creating a new portfolio.
/// </summary>
public class CreatePortfolioRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = "AUD";
    public bool IsDefault { get; set; }
}

/// <summary>
/// Payload for updating an existing portfolio. All fields optional (server-side partial update).
/// </summary>
public class UpdatePortfolioRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Currency { get; set; }
    public bool? IsDefault { get; set; }
}
