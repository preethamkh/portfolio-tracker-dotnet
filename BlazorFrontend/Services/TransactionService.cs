using BlazorFrontend.Models;
using BlazorFrontend.Services.Interfaces;

namespace BlazorFrontend.Services;

/// <summary>
/// Transaction endpoints. Routes: /api/users/{userId}/portfolios/{portfolioId}/transactions
/// The backend recalculates holding positions when transactions change,
/// so the UI only needs to refresh holdings after a successful call.
/// </summary>
public class TransactionService(IApiClient apiClient, IAuthService authService, ILogger<TransactionService> logger)
    : ITransactionService
{
    public async Task<ApiResult<List<Transaction>>> GetPortfolioTransactionsAsync(Guid portfolioId)
    {
        var url = $"api/users/{authService.UserId}/portfolios/{portfolioId}/transactions";
        var result = await apiClient.GetAsync<List<Transaction>>(url);
        return result.IsSuccess
            ? ApiResult<List<Transaction>>.Success(result.Value ?? [])
            : ApiResult<List<Transaction>>.Failure(result.Error!, result.StatusCode);
    }

    public async Task<ApiResult<Transaction>> CreateTransactionAsync(Guid portfolioId, CreateTransactionRequest request)
    {
        logger.LogInformation("Creating {Type} transaction for holding {HoldingId}",
            request.TransactionType, request.HoldingId);

        var url = $"api/users/{authService.UserId}/portfolios/{portfolioId}/transactions";
        return await apiClient.PostAsync<Transaction>(url, request);
    }

    public async Task<ApiResult<Unit>> DeleteTransactionAsync(Guid transactionId)
    {
        logger.LogInformation("Deleting transaction {TransactionId}", transactionId);
        var url = $"api/users/{authService.UserId}/transactions/{transactionId}";
        return await apiClient.DeleteAsync<Unit>(url);
    }
}
