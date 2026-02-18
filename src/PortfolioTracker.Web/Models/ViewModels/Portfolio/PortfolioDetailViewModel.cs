namespace PortfolioTracker.Web.Models.ViewModels.Portfolio;

// This ViewModel is used by Portfolio/Detail.cshtml
// It combines the portfolio summary with its list of holdings
public class PortfolioDetailViewModel
{
    public PortfolioViewModel Portfolio { get; set; } = new();
    public List<HoldingViewModel> Holdings { get; set; } = new();
}