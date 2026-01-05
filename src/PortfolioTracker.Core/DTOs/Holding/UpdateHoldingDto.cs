namespace PortfolioTracker.Core.DTOs.Holding;

/// <summary>
/// Update holding shares and average cost.
/// Used when user manually adjust position without a transaction.
/// </summary>
public class UpdateHoldingDto
{
    public decimal TotalShares { get; set; }
    public decimal? AverageCost { get; set; }
}
