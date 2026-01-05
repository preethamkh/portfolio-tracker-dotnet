namespace PortfolioTracker.Core.DTOs.Holding;

/// <summary>
/// Create a new holding in a portfolio.
/// User selects a security and specifies initial position.
/// </summary>
public class CreateHoldingDto
{
    // todo: more fields later (need to think about this)
    // example: brokerage amount
    public Guid SecurityId { get; set; }
    public decimal TotalShares { get; set; }
    public decimal? AverageCost { get; set; }
}
