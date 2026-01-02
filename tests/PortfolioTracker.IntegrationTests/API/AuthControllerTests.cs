using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.DTOs.Auth;
using PortfolioTracker.IntegrationTests.Fixtures;
using PortfolioTracker.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;

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

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        await TestDataBuilder.CreateUser(Context, email: "preetham@test.com");

        var request = new RegisterRequest
        {
            Email = "preetham@test.com",
            Password = "TestPass123!",
            FullName = "Preetham K H"
        };

        // Act
        var response = await Client.PostAsJsonAsync("api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "weak", // Too weak!
            FullName = "Test"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "preetham@test.com",
            Password = "TestPass123!",
            FullName = "Preetham K H"
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "preetham@test.com",
            Password = "TestPass123!",
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.ReadAsJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Email.Should().Be("preetham@test.com");
        authResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "preetham@test.com",
            Password = "TestPass123!",
            FullName = "Preetham K H"
        });

        var loginRequest = new LoginRequest
        {
            Email = "preetham@test.com",
            Password = "WrongPass123!" // Wrong password
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "SomePass123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUserInfo()
    {
        // Arrange - Register and login
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "preetham@test.com",
            Password = "TestPass123!",
            FullName = "Preetham K H"
        });

        var authResponse = await registerResponse.ReadAsJsonAsync<AuthResponse>();

        // Set token in Authorization header
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Act
        var response = await Client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userInfo = await response.ReadAsJsonAsync<UserInfo>();
        userInfo.Should().NotBeNull();
        userInfo.Email.Should().Be("preetham@test.com");
    }

    [Fact]
    public async Task GetMe_WithoutToken_RetunsUnauthorized()
    {
        // Act - No token set
        var response = await Client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
