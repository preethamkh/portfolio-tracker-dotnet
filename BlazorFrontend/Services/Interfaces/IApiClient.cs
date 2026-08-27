using BlazorFrontend.Models;

namespace BlazorFrontend.Services.Interfaces;

/// <summary>
/// Low-level HTTP facade. Centralizes auth headers, JSON options and error
/// mapping so feature services stay thin and focused on their endpoints.
/// </summary>
public interface IApiClient
{
    Task<ApiResult<T>> GetAsync<T>(string url);
    Task<ApiResult<T>> PostAsync<T>(string url, object body);
    Task<ApiResult<T>> PutAsync<T>(string url, object body);
    Task<ApiResult<T>> DeleteAsync<T>(string url);
}
