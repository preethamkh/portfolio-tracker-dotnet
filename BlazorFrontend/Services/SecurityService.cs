using BlazorFrontend.Models;
using BlazorFrontend.Services.Interfaces;

namespace BlazorFrontend.Services;

/// <summary>
/// Security master-data endpoints (global, not user-scoped):
/// /api/securities
/// </summary>
public class SecurityService(IApiClient apiClient, ILogger<SecurityService> logger) : ISecurityService
{
    public async Task<ApiResult<List<Security>>> SearchSecuritiesAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ApiResult<List<Security>>.Success([]);
        }

        var url = $"api/securities/search?query={Uri.EscapeDataString(query)}&limit={limit}";
        var result = await apiClient.GetAsync<List<Security>>(url);
        return result.IsSuccess
            ? ApiResult<List<Security>>.Success(result.Value ?? [])
            : ApiResult<List<Security>>.Failure(result.Error!, result.StatusCode);
    }

    public async Task<ApiResult<Security>> GetSecurityBySymbolAsync(string symbol) =>
        await apiClient.GetAsync<Security>($"api/securities/symbol/{Uri.EscapeDataString(symbol)}");

    public async Task<ApiResult<Security>> GetOrCreateSecurityAsync(string symbol)
    {
        logger.LogInformation("Get-or-create security {Symbol}", symbol);
        return await apiClient.PostAsync<Security>("api/securities/get-or-create", new { symbol });
    }
}
