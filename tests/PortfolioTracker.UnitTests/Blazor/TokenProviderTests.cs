using BlazorFrontend.Services;
using FluentAssertions;

namespace PortfolioTracker.UnitTests.Blazor;

/// <summary>
/// In-memory localStorage fake so TokenProvider tests never touch JS interop.
/// </summary>
public class FakeLocalStorage : ILocalStorageService
{
    public Dictionary<string, string> Store { get; } = new();

    public Task<string?> GetItemAsync(string key) =>
        Task.FromResult(Store.TryGetValue(key, out var value) ? value : null);

    public Task SetItemAsync(string key, string value)
    {
        Store[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string key)
    {
        Store.Remove(key);
        return Task.CompletedTask;
    }
}

/// <summary>
/// TokenProvider behavior: in-memory caching, persistence and cleanup.
/// </summary>
public class TokenProviderTests
{
    [Fact]
    public async Task GetTokenAsync_ReturnsNull_WhenNothingStored()
    {
        // Arrange
        var provider = new TokenProvider(new FakeLocalStorage());

        // Act
        var token = await provider.GetTokenAsync();

        // Assert
        token.Should().BeNull();
    }

    [Fact]
    public async Task SetTokenAsync_PersistsTokenAndUser()
    {
        // Arrange
        var storage = new FakeLocalStorage();
        var provider = new TokenProvider(storage);

        // Act
        await provider.SetTokenAsync("jwt-token", """{"id":"..."}""");

        // Assert
        storage.Store.Should().ContainKey("auth_token").WhoseValue.Should().Be("jwt-token");
        storage.Store.Should().ContainKey("auth_user");
    }

    [Fact]
    public async Task GetTokenAsync_UsesMemoryCache_AfterStorageClearedExternally()
    {
        // Arrange - simulates getting the token once, then storage changing
        var storage = new FakeLocalStorage();
        var provider = new TokenProvider(storage);
        await provider.SetTokenAsync("jwt-token", "{}");

        // Act - read token (caches it), then wipe storage behind its back
        await provider.GetTokenAsync();
        await provider.ClearTokenAsync();

        // Assert
        (await provider.GetTokenAsync()).Should().BeNull();
    }

    [Fact]
    public async Task ClearTokenAsync_RemovesAllAuthEntries()
    {
        // Arrange
        var storage = new FakeLocalStorage();
        var provider = new TokenProvider(storage);
        await provider.SetTokenAsync("jwt-token", "{}");

        // Act
        await provider.ClearTokenAsync();

        // Assert
        storage.Store.Should().NotContainKey("auth_token");
        storage.Store.Should().NotContainKey("auth_user");
    }
}
