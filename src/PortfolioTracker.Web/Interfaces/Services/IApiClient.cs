using PortfolioTracker.Web.Models.ViewModels.Auth;
using PortfolioTracker.Web.Models.ViewModels.Holdings;
using PortfolioTracker.Web.Models.ViewModels.Portfolio;
using PortfolioTracker.Web.Models.ViewModels.Securities;
using PortfolioTracker.Web.Models.ViewModels.Transactions;

namespace PortfolioTracker.Web.Interfaces.Services;

public interface IApiClient
{
    Task<AuthResponseDto?> LoginAsync(LoginViewModel model);
    Task<AuthResponseDto?> RegisterAsync(RegisterViewModel model);

    Task<List<PortfolioViewModel>> GetPortfoliosAsync();
    Task<PortfolioViewModel?> GetPortfolioAsync(int id);
    Task<PortfolioViewModel?> CreatePortfolioAsync(CreatePortfolioViewModel model);
    Task<bool> DeletePortfolioAsync(int id);

    Task<List<HoldingViewModel>> GetHoldingsAsync(int portfolioId);
    Task<HoldingViewModel?> GetHoldingAsync(int id);
    Task<HoldingViewModel?> CreateHoldingAsync(CreateHoldingViewModel model);
    Task<bool> DeleteHoldingAsync(int id);

    Task<List<TransactionViewModel>> GetTransactionsAsync(int holdingId);
    Task<TransactionViewModel?> CreateTransactionAsync(CreateTransactionViewModel model);
    Task<bool> DeleteTransactionAsync(int id);

    Task<List<SecurityViewModel>> SearchSecuritiesAsync(string query);
    Task<SecurityViewModel?> GetSecurityAsync(string symbol);
}
