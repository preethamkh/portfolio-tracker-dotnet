using System.Security.Claims;
using BlazorFrontend.Models;
using BlazorFrontend.Models.Auth;
using BlazorFrontend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorFrontend.Services;

/// <summary>
/// Handles authentication flows (login, register, logout, session restore)
/// and doubles as the app's AuthenticationStateProvider so [Authorize] and
/// AuthorizeView work out of the box.
///
/// The user id is exposed here because API routes are user-scoped:
/// /api/users/{userId}/portfolios/...
/// </summary>
public class AuthService(IAuthApi authApi, ITokenProvider tokenProvider, ILogger<AuthService> logger)
    : AuthenticationStateProvider, IAuthService
{
    private static readonly ClaimsIdentity AnonymousIdentity = new();

    /// <summary>In-memory copy of the signed-in user (null when logged out).</summary>
    private UserInfo? _currentUser;

    /// <summary>Guards session restoration so it runs exactly once.</summary>
    private Task _initializeTask = Task.CompletedTask;

    public UserInfo? CurrentUser => _currentUser;

    /// <summary>
    /// Convenience accessor used by feature services to build API routes.
    /// Throws rather than returning null to surface programming errors early.
    /// </summary>
    public Guid UserId =>
        _currentUser?.Id ?? throw new InvalidOperationException("No authenticated user");

    public bool IsAuthenticated => _currentUser is not null;

    // ------------------------------------------------------------------
    // IAuthService
    // ------------------------------------------------------------------

    /// <summary>
    /// Restores the session from persisted storage. Must be awaited before
    /// the router evaluates authorization (wired up in Program.cs).
    /// </summary>
    public async Task InitializeAsync()
    {
        // Only restore once even if called from multiple places
        if (_initializeTask.IsCompleted && _currentUser is not null)
        {
            return;
        }

        _initializeTask = RestoreSessionAsync();
        await _initializeTask;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await authApi.LoginAsync(request);

        await ApplyAuthStateAsync(response);
        logger.LogInformation("User {Email} logged in", response.User.Email);

        return response;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await authApi.RegisterAsync(request);

        // Auto-login after successful registration (API returns a token)
        await ApplyAuthStateAsync(response);
        logger.LogInformation("User {Email} registered", response.User.Email);

        return response;
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        await tokenProvider.ClearTokenAsync();
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(AnonymousIdentity))));

        logger.LogInformation("User logged out");
    }

    // ------------------------------------------------------------------
    // AuthenticationStateProvider
    // ------------------------------------------------------------------

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = _currentUser is not null
            ? CreatePrincipal(_currentUser)
            : new ClaimsPrincipal(AnonymousIdentity);

        return Task.FromResult(new AuthenticationState(principal));
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private async Task RestoreSessionAsync()
    {
        var token = await tokenProvider.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return; // not signed in
        }

        // Ask the API who this token belongs to. This both validates the
        // token (it may have expired) and gives us the current user info.
        var meResult = await authApi.GetCurrentUserAsync();
        if (meResult.IsSuccess && meResult.Value is not null)
        {
            _currentUser = meResult.Value;
        }
        else
        {
            // Expired/invalid token - clean up so we don't retry forever
            logger.LogInformation("Stored token is no longer valid; clearing session");
            _currentUser = null;
            await tokenProvider.ClearTokenAsync();
        }
    }

    private async Task ApplyAuthStateAsync(AuthResponse response)
    {
        _currentUser = response.User;

        // Persist both token and user (user avoids a /me roundtrip on refresh)
        await tokenProvider.SetTokenAsync(response.Token, SerializeUser(response.User));

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(CreatePrincipal(response.User))));
    }

    private static ClaimsPrincipal CreatePrincipal(UserInfo user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName ?? user.Email)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "jwt"));
    }

    private static string SerializeUser(UserInfo user) =>
        System.Text.Json.JsonSerializer.Serialize(user,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
}
