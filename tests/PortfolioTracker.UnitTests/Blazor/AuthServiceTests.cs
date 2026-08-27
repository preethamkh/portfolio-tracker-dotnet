using System.Net;
using BlazorFrontend.Models;
using BlazorFrontend.Models.Auth;
using BlazorFrontend.Services;
using BlazorFrontend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace PortfolioTracker.UnitTests.Blazor;

/// <summary>
/// AuthService behavior: login/logout state transitions, session restore
/// and cleanup of invalid persisted tokens.
/// </summary>
public class AuthServiceTests
{
    private static readonly UserInfo TestUser = new()
    {
        Id = Guid.Parse("9d5e9b1e-6f4e-4c9a-9a5b-3d3c3f9f1a01"),
        Email = "test@example.com",
        FullName = "Test User"
    };

    private static AuthResponse SuccessResponse() => new()
    {
        Token = "jwt-token",
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        User = TestUser
    };

    private readonly Mock<IAuthApi> _authApi = new();
    private readonly Mock<ITokenProvider> _tokenProvider = new();

    private AuthService CreateService() =>
        new(_authApi.Object, _tokenProvider.Object, NullLogger<AuthService>.Instance);

    [Fact]
    public async Task LoginAsync_StoresToken_AndTransitionsToAuthenticated()
    {
        // Arrange
        _authApi.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(SuccessResponse());
        var service = CreateService();

        // Act
        var response = await service.LoginAsync(new LoginRequest());

        // Assert
        response.Token.Should().Be("jwt-token");
        service.IsAuthenticated.Should().BeTrue();
        service.CurrentUser!.Email.Should().Be(TestUser.Email);
        service.UserId.Should().Be(TestUser.Id);
        _tokenProvider.Verify(p => p.SetTokenAsync("jwt-token", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_NotifiesAuthenticationStateChanged()
    {
        // Arrange
        _authApi.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(SuccessResponse());
        var service = CreateService();
        var stateChanged = false;
        service.AuthenticationStateChanged += _ => stateChanged = true;

        // Act
        await service.LoginAsync(new LoginRequest());

        // Assert
        stateChanged.Should().BeTrue();
        var state = await service.GetAuthenticationStateAsync();
        state.User.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAsync_ClearsSession_AndTransitionsToAnonymous()
    {
        // Arrange
        _authApi.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(SuccessResponse());
        var service = CreateService();
        await service.LoginAsync(new LoginRequest());

        // Act
        await service.LogoutAsync();

        // Assert
        service.IsAuthenticated.Should().BeFalse();
        _tokenProvider.Verify(p => p.ClearTokenAsync(), Times.Once);
        var state = await service.GetAuthenticationStateAsync();
        state.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_RestoresSession_FromValidToken()
    {
        // Arrange
        _tokenProvider.Setup(p => p.GetTokenAsync()).ReturnsAsync("jwt-token");
        _authApi.Setup(a => a.GetCurrentUserAsync())
            .ReturnsAsync(ApiResult<UserInfo>.Success(TestUser));
        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.IsAuthenticated.Should().BeTrue();
        service.UserId.Should().Be(TestUser.Id);
    }

    [Fact]
    public async Task InitializeAsync_ClearsSession_WhenTokenIsInvalid()
    {
        // Arrange - stored token no longer valid (e.g. expired)
        _tokenProvider.Setup(p => p.GetTokenAsync()).ReturnsAsync("expired-token");
        _authApi.Setup(a => a.GetCurrentUserAsync())
            .ReturnsAsync(ApiResult<UserInfo>.Failure("Unauthorized", HttpStatusCode.Unauthorized));
        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert - session cleaned up so we don't keep sending a dead token
        service.IsAuthenticated.Should().BeFalse();
        _tokenProvider.Verify(p => p.ClearTokenAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_DoesNothing_WhenNoTokenStored()
    {
        // Arrange
        _tokenProvider.Setup(p => p.GetTokenAsync()).ReturnsAsync((string?)null);
        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.IsAuthenticated.Should().BeFalse();
        _authApi.Verify(a => a.GetCurrentUserAsync(), Times.Never);
    }
}
