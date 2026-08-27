using Microsoft.JSInterop;

namespace BlazorFrontend.Services;

/// <summary>
/// Abstraction over JWT token persistence so the rest of the codebase
/// never touches JS interop directly and can be unit-tested with a fake.
/// </summary>
public interface ITokenProvider
{
    /// <summary>Gets the current token, or null if not signed in.</summary>
    Task<string?> GetTokenAsync();

    /// <summary>Persists the token (and the current user) for future sessions.</summary>
    Task SetTokenAsync(string token, string userJson);

    /// <summary>Removes the token (and user) on logout.</summary>
    Task ClearTokenAsync();
}

/// <summary>
/// localStorage-backed token storage for Blazor WebAssembly.
/// Also keeps an in-memory copy so the first render (before JS interop
/// completes) can still make authenticated calls once initialized.
/// Note: localStorage is readable by client-side scripts. Acceptable for
/// this app's threat model; consider HttpOnly cookies for higher security.
/// </summary>
public class TokenProvider(ILocalStorageService storage) : ITokenProvider
{
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";

    private string? _cachedToken;

    public async Task<string?> GetTokenAsync()
    {
        // Prefer the in-memory copy; fall back to storage (page refresh case)
        return _cachedToken ??= await storage.GetItemAsync(TokenKey);
    }

    public async Task SetTokenAsync(string token, string userJson)
    {
        _cachedToken = token;
        await storage.SetItemAsync(TokenKey, token);
        await storage.SetItemAsync(UserKey, userJson);
    }

    public async Task ClearTokenAsync()
    {
        _cachedToken = null;
        await storage.RemoveItemAsync(TokenKey);
        await storage.RemoveItemAsync(UserKey);
    }
}

/// <summary>
/// Minimal localStorage wrapper. Isolated so it can be faked in tests and
/// so JS interop failures (e.g., prerendering) degrade gracefully.
/// </summary>
public interface ILocalStorageService
{
    Task<string?> GetItemAsync(string key);
    Task SetItemAsync(string key, string value);
    Task RemoveItemAsync(string key);
}

public class LocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
{
    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSException)
        {
            // JS interop unavailable (e.g., prerender) - treat as not signed in
            return null;
        }
    }

    public async Task SetItemAsync(string key, string value) =>
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (JSException)
        {
            // ignore - nothing to clean up
        }
    }
}
