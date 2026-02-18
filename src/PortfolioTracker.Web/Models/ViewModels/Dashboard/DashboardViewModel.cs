using PortfolioTracker.Web.Models.ViewModels.Portfolio;
using PortfolioTracker.Web.Models.ViewModels.Transactions;

namespace PortfolioTracker.Web.Models.ViewModels.Dashboard;

public class DashboardViewModel
{
    public List<PortfolioViewModel> Portfolios { get; set; } = new();
    public decimal TotalValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal TotalGainLossPercent { get; set; }
    public int TotalHoldings { get; set; }
    public List<TransactionViewModel> RecentTransactions { get; set; } = new();
}
