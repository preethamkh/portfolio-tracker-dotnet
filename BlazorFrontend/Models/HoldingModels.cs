namespace BlazorFrontend.Models;

/// <summary>
/// Holding (position in a security) with current valuation.
/// Mirrors the backend's HoldingDto.
/// </summary>
public class Holding
{
    public Guid HoldingId { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid SecurityId { get; set; }

    // Denormalized security info (avoids extra lookups when rendering lists)
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public string SecurityType { get; set; } = string.Empty;

    public decimal TotalShares { get; set; }
    public decimal? AverageCost { get; set; }

    // Current valuation (calculated server-side from real-time prices)
    public decimal? CurrentPrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? UnrealizedGainLoss { get; set; }
    public decimal? UnrealizedGainLossPercent { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Payload for creating a new holding. Mirrors the backend's CreateHoldingDto.
/// The backend expects the security to already exist locally, so the UI
/// flow is: search security (get-or-create) -> create holding.
/// </summary>
public class CreateHoldingRequest
{
    public Guid SecurityId { get; set; }
    public decimal TotalShares { get; set; }
    public decimal? AverageCost { get; set; }
}
