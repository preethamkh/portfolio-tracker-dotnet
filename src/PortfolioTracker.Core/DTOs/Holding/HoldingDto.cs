namespace PortfolioTracker.Core.DTOs.Holding;

/// <summary>
/// Full holding information including security details and current valuation.
/// Used when displaying portfolio holdings to the user.
/// </summary>
public class HoldingDto
{
    public Guid HoldingId { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid SecurityId { get; set; }

    // Security information (denormalized for convenience)
    // Performance: It avoids extra database or service calls to fetch security details every time holdings are displayed. All needed info is in one object.
    // Convenience: The UI or API consumer gets all relevant data in a single DTO, making it easier to render portfolio holdings without additional lookups.
    // Snapshot Consistency: Holdings may reflect the state of a security at the time of acquisition, which could differ from current security data. Denormalized fields can preserve this historical context.
    // Denormalization here is a trade-off favoring read efficiency and simplicity for portfolio display scenarios, at the cost of some redundancy.
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public string SecurityType { get; set; } = string.Empty;

    public decimal TotalShares { get; set; }
    public decimal? AverageCost { get; set; }

    // Current valuation (calculated from real-time prices)
    public decimal? CurrentPrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? UnrealizedGainLoss { get; set; }
    public decimal? UnrealizedGainLossPercent { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
