namespace PortfolioTracker.Core.DTOs.Portfolio;

/// <summary>
/// DTO for Portfolio responses.
/// Represents a user's investment portfolio.
/// </summary>
public class PortfolioDto
{
    /// <summary>
    /// Portfolio unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who owns this portfolio.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Portfolio name (e.g., "Retirement Fund", "Trading Account").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the portfolio's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Portfolio base currency (e.g., "AUD", "USD").
    /// </summary>
    public string Currency { get; set; } = "AUD";

    /// <summary>
    /// Whether this is the user's default portfolio.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// When this portfolio was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last time this portfolio was modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Number of holdings in this portfolio.
    /// Useful for displaying in lists without loading all holdings.
    /// </summary>
    public int HoldingsCount { get; set; }

    // Note: We don't include the full Holdings collection here
    // That would be retrieved via a separate endpoint: GET /api/portfolios/{id}/holdings
}