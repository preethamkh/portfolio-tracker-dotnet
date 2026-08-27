using BlazorFrontend.Models;
using BlazorFrontend.Services.Interfaces;

namespace BlazorFrontend.Services;

/// <summary>
/// Holdings endpoints. Routes: /api/users/{userId}/portfolios/{portfolioId}/holdings
/// (Creating/updating holdings is done via transactions in this app's flow,
/// which automatically recalculate position size and average cost.)
/// </summary>
public class HoldingService(IApiClient apiClient, IAuthService authService) : IHoldingService
{
    private string BaseRoute(Guid portfolioId) =>
        $"api/users/{authService.UserId}/portfolios/{portfolioId}/holdings";

    public async Task<ApiResult<List<Holding>>> GetHoldingsAsync(Guid portfolioId)
    {
        var result = await apiClient.GetAsync<List<Holding>>(BaseRoute(portfolioId));
        return result.IsSuccess
            ? ApiResult<List<Holding>>.Success(result.Value ?? [])
            : ApiResult<List<Holding>>.Failure(result.Error!, result.StatusCode);
    }

    public async Task<ApiResult<Holding>> GetHoldingAsync(Guid portfolioId, Guid holdingId) =>
        await apiClient.GetAsync<Holding>($"{BaseRoute(portfolioId)}/{holdingId}");

    public async Task<ApiResult<Holding>> CreateHoldingAsync(Guid portfolioId, CreateHoldingRequest request) =>
        await apiClient.PostAsync<Holding>(BaseRoute(portfolioId), request);
}
