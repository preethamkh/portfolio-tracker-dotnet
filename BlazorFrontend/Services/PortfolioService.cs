using BlazorFrontend.Models;
using BlazorFrontend.Services.Interfaces;

namespace BlazorFrontend.Services;

/// <summary>
/// Portfolio endpoints. All routes are user-scoped per the API contract:
/// /api/users/{userId}/portfolios
/// </summary>
public class PortfolioService(IApiClient apiClient, IAuthService authService, ILogger<PortfolioService> logger)
    : IPortfolioService
{
    private string BaseRoute => $"api/users/{authService.UserId}/portfolios";

    public async Task<ApiResult<List<Portfolio>>> GetPortfoliosAsync()
    {
        var result = await apiClient.GetAsync<List<Portfolio>>(BaseRoute);
        return result.IsSuccess
            ? ApiResult<List<Portfolio>>.Success(result.Value ?? [])
            : ApiResult<List<Portfolio>>.Failure(result.Error!, result.StatusCode);
    }

    public async Task<ApiResult<Portfolio>> GetPortfolioAsync(Guid portfolioId) =>
        await apiClient.GetAsync<Portfolio>($"{BaseRoute}/{portfolioId}");

    public async Task<ApiResult<Portfolio>> CreatePortfolioAsync(CreatePortfolioRequest request)
    {
        logger.LogInformation("Creating portfolio {Name}", request.Name);
        return await apiClient.PostAsync<Portfolio>(BaseRoute, request);
    }

    public async Task<ApiResult<Portfolio>> UpdatePortfolioAsync(Guid portfolioId, UpdatePortfolioRequest request) =>
        await apiClient.PutAsync<Portfolio>($"{BaseRoute}/{portfolioId}", request);

    public async Task<ApiResult<Unit>> DeletePortfolioAsync(Guid portfolioId)
    {
        logger.LogInformation("Deleting portfolio {PortfolioId}", portfolioId);
        return await apiClient.DeleteAsync<Unit>($"{BaseRoute}/{portfolioId}");
    }

    public async Task<ApiResult<Unit>> SetDefaultPortfolioAsync(Guid portfolioId) =>
        await apiClient.PostAsync<Unit>($"{BaseRoute}/{portfolioId}/set-default", new { });
}
