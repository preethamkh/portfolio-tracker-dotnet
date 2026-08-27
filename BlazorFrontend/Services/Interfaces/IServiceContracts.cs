using BlazorFrontend.Models;
using BlazorFrontend.Models.Auth;

namespace BlazorFrontend.Services.Interfaces;

/// <summary>
/// Raw auth endpoints. Separated from IAuthService so the latter can own
/// session state without knowing HTTP details.
/// </summary>
public interface IAuthApi
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<ApiResult<UserInfo>> GetCurrentUserAsync();
}

/// <summary>
/// High-level authentication/session state used by pages and services.
/// </summary>
public interface IAuthService
{
    /// <summary>Currently signed-in user, or null when logged out.</summary>
    UserInfo? CurrentUser { get; }

    /// <summary>User id for building API routes; throws when logged out.</summary>
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    /// <summary>Restores a persisted session (call once at startup).</summary>
    Task InitializeAsync();

    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
}

/// <summary>Portfolio management endpoints.</summary>
public interface IPortfolioService
{
    Task<ApiResult<List<Portfolio>>> GetPortfoliosAsync();
    Task<ApiResult<Portfolio>> GetPortfolioAsync(Guid portfolioId);
    Task<ApiResult<Portfolio>> CreatePortfolioAsync(CreatePortfolioRequest request);
    Task<ApiResult<Portfolio>> UpdatePortfolioAsync(Guid portfolioId, UpdatePortfolioRequest request);
    Task<ApiResult<Unit>> DeletePortfolioAsync(Guid portfolioId);
    Task<ApiResult<Unit>> SetDefaultPortfolioAsync(Guid portfolioId);
}

/// <summary>Holdings endpoints (positions inside a portfolio).</summary>
public interface IHoldingService
{
    Task<ApiResult<List<Holding>>> GetHoldingsAsync(Guid portfolioId);
    Task<ApiResult<Holding>> GetHoldingAsync(Guid portfolioId, Guid holdingId);
    Task<ApiResult<Holding>> CreateHoldingAsync(Guid portfolioId, CreateHoldingRequest request);
}

/// <summary>Transaction endpoints (buy/sell activity).</summary>
public interface ITransactionService
{
    Task<ApiResult<List<Transaction>>> GetPortfolioTransactionsAsync(Guid portfolioId);
    Task<ApiResult<Transaction>> CreateTransactionAsync(Guid portfolioId, CreateTransactionRequest request);
    Task<ApiResult<Unit>> DeleteTransactionAsync(Guid transactionId);
}

/// <summary>Security master-data endpoints (search / lookup).</summary>
public interface ISecurityService
{
    Task<ApiResult<List<Security>>> SearchSecuritiesAsync(string query, int limit = 10);
    Task<ApiResult<Security>> GetSecurityBySymbolAsync(string symbol);
    Task<ApiResult<Security>> GetOrCreateSecurityAsync(string symbol);
}

/// <summary>
/// Explicit "no data" marker for endpoints that return 204 No Content.
/// Makes intent clear at call sites versus a null value.
/// </summary>
public readonly record struct Unit;
