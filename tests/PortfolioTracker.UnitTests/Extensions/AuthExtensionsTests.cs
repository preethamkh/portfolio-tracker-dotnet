using System.Security.Claims;
using FluentAssertions;
using PortfolioTracker.API.Extensions;

namespace PortfolioTracker.UnitTests.Extensions;

public class AuthExtensionsTests
{
    [Fact]
    public void GetAuthenticatedUserId_ReturnsGuid_WhenClaimExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetAuthenticatedUserId_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetAuthenticatedUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAuthenticatedUserId_ReturnsNull_WhenClaimIsNotAGuid()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetAuthenticatedUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IsAuthorizedForUser_ReturnsTrue_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.IsAuthorizedForUser(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthorizedForUser_ReturnsFalse_WhenUserIsNotAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.IsAuthorizedForUser(otherUserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthorizedForUser_ReturnsFalse_WhenUserIdClaimMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.IsAuthorizedForUser(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}