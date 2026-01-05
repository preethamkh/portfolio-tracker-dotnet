namespace PortfolioTracker.Core.DTOs.Holding;

/// <summary>
/// Lightweight summary representation of a holding for list views.
/// Used to list without full details.
/// </summary>
public class HoldingSummaryDto
{
    public Guid HoldingSummaryId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public decimal TotalShares { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? UnrealizedGainLossPercent { get; set; }
}
