namespace PortfolioTracker.Web.Models.ViewModels.Portfolio;

public class PortfolioViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal TotalGainLossPercent { get; set; }
    public int HoldingsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
