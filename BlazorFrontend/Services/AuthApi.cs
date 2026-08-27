using BlazorFrontend.Models;
using BlazorFrontend.Models.Auth;
using BlazorFrontend.Services.Interfaces;

namespace BlazorFrontend.Services;

/// <summary>
/// Thin wrapper over the /api/auth endpoints. Errors are mapped to
/// InvalidOperationException with a user-friendly message so pages can
/// display them directly in the login/register forms.
/// </summary>
public class AuthApi(IApiClient apiClient) : IAuthApi
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var result = await apiClient.PostAsync<AuthResponse>("api/auth/login", request);
        return result.IsSuccess
            ? result.Value!
            : throw new InvalidOperationException(result.Error ?? "Login failed");
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var result = await apiClient.PostAsync<AuthResponse>("api/auth/register", request);
        return result.IsSuccess
            ? result.Value!
            : throw new InvalidOperationException(result.Error ?? "Registration failed");
    }

    public async Task<ApiResult<UserInfo>> GetCurrentUserAsync()
    {
        var result = await apiClient.GetAsync<UserInfo>("api/auth/me");
        if (result.IsSuccess)
        {
            return ApiResult<UserInfo>.Success(result.Value!);
        }

        // Preserve the original status code (important: expired token = 401)
        return ApiResult<UserInfo>.Failure(result.Error ?? "Not authenticated", result.StatusCode);
    }
}
