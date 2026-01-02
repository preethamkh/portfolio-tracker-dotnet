using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.DTOs.Auth;
using PortfolioTracker.IntegrationTests.Fixtures;
using PortfolioTracker.IntegrationTests.Helpers;

namespace PortfolioTracker.IntegrationTests.API;

public class AuthControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ReturnsTokenAndUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "preetham@test.com",
            Password = "TestPass123!",
            FullName = "Preetham K H"
        };

        // Act
        var response = await Client.PostAsJsonAsync("api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.ReadAsJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Email.Should().Be("preetham@test.com");
        authResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // verify password was hashed
        var user = await Context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == "preetham@test.com");
        user.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("TestPass123!", user.PasswordHash).Should().BeTrue();
        user.PasswordHash.Should().NotBe("TestPass123!");
        user.PasswordHash.Should().StartWith("$2"); // BCrypt hash
    }

    #endregion
}
