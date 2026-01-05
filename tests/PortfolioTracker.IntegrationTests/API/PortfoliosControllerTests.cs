using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.DTOs.Portfolio;
using PortfolioTracker.IntegrationTests.Fixtures;
using PortfolioTracker.IntegrationTests.Helpers;
using System.Net;
using PortfolioTracker.Core.Helpers;

namespace PortfolioTracker.IntegrationTests.API;

public class PortfoliosControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region GET /api/users/{userId}/portfolios - Get User Portfolios

    [Fact]
    public async Task GetUserPortfolios_WhenNoPortfolios_ReturnsEmptyList()
    {
        // Arrange: Register and authenticate a user
        var authResponse = await RegisterAndAuthenticateAsync();
        var userId = authResponse.User.Id;

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolios = await response.ReadAsJsonAsync<List<PortfolioDto>>();
        portfolios.Should().NotBeNull();
        portfolios.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPortfolios_WhenPortfoliosExist_ReturnsOnlyUserPortfolios()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;

        await TestDataBuilder.CreatePortfolios(Context, userId, 3);

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolios = await response.ReadAsJsonAsync<List<PortfolioDto>>();
        portfolios.Should().NotBeNull();
        portfolios.Should().HaveCount(3);
        portfolios.Should().OnlyContain(p => p.UserId == userId);
    }

    [Fact]
    public async Task GetUserPortfolios_WhenPortfoliosExist_OrdersByDefaultFirstThenName()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;

        await TestDataBuilder.CreatePortfolio(Context, userId, "Savings", isDefault: false);
        await TestDataBuilder.CreatePortfolio(Context, userId, "Day Trading", isDefault: false);
        await TestDataBuilder.CreatePortfolio(Context, userId, "Retirement", isDefault: true);

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolios = await response.ReadAsJsonAsync<List<PortfolioDto>>();
        portfolios.Should().NotBeNull();

        portfolios[0].IsDefault.Should().BeTrue();

        // order by default first, then name
        portfolios[0].Name.Should().Be("Retirement");
        portfolios[1].Name.Should().Be("Day Trading");
        portfolios[2].Name.Should().Be("Savings");
    }

    #endregion

    #region GET /api/users/{userId}/portfolios/{portfolioId} - Get Portfolio By ID

    [Fact]
    public async Task GetPortfolio_WhenExists_ReturnsPortfolio()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Retirement");

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios/{portfolio.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedPortfolio = await response.ReadAsJsonAsync<PortfolioDto>();
        returnedPortfolio.Should().NotBeNull();
        returnedPortfolio.Id.Should().Be(portfolio.Id);
        returnedPortfolio.Name.Should().Be("Retirement");
        returnedPortfolio.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetPortfolio_WhenDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPortfolio_WhenBelongsToOtherUser_ReturnsNotFound()
    {
        // Arrange - Authorization test
        var user1Id = (await RegisterAndAuthenticateAsync()).User.Id;
        var user2Id = (await RegisterAndAuthenticateAsync()).User.Id;
        var user1Portfolio = await TestDataBuilder.CreatePortfolio(Context, user1Id);

        // Act - User2 tries to access User1's portfolio
        var response = await Client.GetAsync($"/api/users/{user2Id}/portfolios/{user1Portfolio.Id}");

        // Assert - Should not find it (authorization check)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/users/{userId}/default - Get Default Portfolio

    [Fact]
    public async Task GetDefaultPortfolio_WhenExists_ReturnsDefaultPortfolio()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        await TestDataBuilder.CreatePortfolio(Context, userId, "Retirement", isDefault: true);
        await TestDataBuilder.CreatePortfolio(Context, userId, "Savings", isDefault: false);

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios/default");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var defaultPortfolio = await response.ReadAsJsonAsync<PortfolioDto>();
        defaultPortfolio.Should().NotBeNull();
        defaultPortfolio.IsDefault.Should().BeTrue();
        defaultPortfolio.Name.Should().Be("Retirement");
    }

    [Fact]
    public async Task GetDefaultPortfolio_WhenDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        await TestDataBuilder.CreatePortfolio(Context, userId, "Savings", isDefault: false);

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios/default");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/users/{userId}/portfolios - Create Portfolio

    [Fact]
    public async Task CreatePortfolio_WithValidData_ReturnsCreatedPortfolio()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        var createDto = new CreatePortfolioDto
        {
            Name = "My Portfolio",
            Description = "Investment portfolio",
            Currency = "AUD",
            IsDefault = true
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/portfolios", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdPortfolio = await response.ReadAsJsonAsync<PortfolioDto>();
        createdPortfolio.Should().NotBeNull();
        createdPortfolio.Name.Should().Be("My Portfolio");
        createdPortfolio.IsDefault.Should().BeTrue();
        createdPortfolio.UserId.Should().Be(userId);

        // Verify in database (bypass cache)
        var portfolioInDb = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == createdPortfolio.Id);

        portfolioInDb.Should().NotBeNull();
        portfolioInDb.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePortfolio_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        await TestDataBuilder.CreatePortfolio(Context, userId, name: "Retirement");

        var createPortfolioDto = new CreatePortfolioDto
        {
            Name = "Retirement", // Duplicate!
            Description = "Retirement Portfolio",
            Currency = "AUD"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/portfolios", createPortfolioDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain($"You already have a portfolio named '{createPortfolioDto.Name}'");
    }

    [Fact]
    public async Task CreatePortfolio_DifferentUsersSameNameAllowed()
    {
        // Arrange - Business rule: Different users CAN have the same portfolio name
        var user1Id = (await RegisterAndAuthenticateAsync()).User.Id;
        var user2Id = (await RegisterAndAuthenticateAsync()).User.Id;

        await TestDataBuilder.CreatePortfolio(Context, user1Id, name: "Retirement");

        var createPortfolioDto = new CreatePortfolioDto
        {
            Name = "Retirement", // Same name, but different user
            Description = "User 2's retirement",
            Currency = "AUD"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{user2Id}/portfolios", createPortfolioDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePortfolio_AsDefault_UnsetsOtherDefaults()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        var existingDefault = await TestDataBuilder.CreatePortfolio(Context, userId,
            name: "Old Default",
            isDefault: true);

        var createDto = new CreatePortfolioDto
        {
            Name = "New Default",
            Description = "This should become default",
            Currency = "AUD",
            IsDefault = true // Request to be default
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/portfolios", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdPortfolio = await response.ReadAsJsonAsync<PortfolioDto>();
        createdPortfolio!.IsDefault.Should().BeTrue();

        // Verify old default is no longer default (bypass cache)
        var oldDefaultInDb = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == existingDefault.Id);

        oldDefaultInDb!.IsDefault.Should().BeFalse();
    }

    #endregion

    #region PUT /api/users/{userId}/portfolios/{portfolioId} - Update Portfolio

    [Fact]
    public async Task UpdatePortfolio_WithValidData_ReturnsUpdatedPortfolio()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId,
            name: "Old Name",
            description: "Old Description");

        var updatePortfolioDto = new UpdatePortfolioDto
        {
            Name = "New Name",
            Description = "New Description"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{userId}/portfolios/{portfolio.Id}", updatePortfolioDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedPortfolio = await response.ReadAsJsonAsync<PortfolioDto>();
        updatedPortfolio!.Name.Should().Be("New Name");
        updatedPortfolio.Description.Should().Be("New Description");

        // Verify in database (bypass cache)
        var portfolioInDb = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == portfolio.Id);
        portfolioInDb!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdatePortfolio_WhenBelongsToOtherUser_ReturnsNotFound()
    {
        // Arrange
        var user1Id = (await RegisterAndAuthenticateAsync()).User.Id;
        var user2Id = (await RegisterAndAuthenticateAsync()).User.Id;
        var user1Portfolio = await TestDataBuilder.CreatePortfolio(Context, user1Id);

        var updateDto = new UpdatePortfolioDto { Name = "Hacked Name" };

        // Act - User2 tries to update User1's portfolio
        var response = await Client.PutAsJsonAsync($"/api/users/{user2Id}/portfolios/{user1Portfolio.Id}", updateDto);

        // Assert - Should not find it (authorization check)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify name wasn't changed (bypass cache)
        var portfolioInDb = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == user1Portfolio.Id);

        portfolioInDb!.Name.Should().NotBe("Hacked Name");
    }

    #endregion

    #region DELETE /api/users/{userId}/portfolios/{portfolioId} - Delete Portfolio

    [Fact]
    public async Task DeletePortfolio_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId);

        // Act
        var response = await Client.DeleteAsync($"/api/users/{userId}/portfolios/{portfolio.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted (bypass cache)
        var portfolioInDb = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == portfolio.Id);

        portfolioInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeletePortfolio_WhenBelongsToOtherUser_ReturnsNotFound()
    {
        // Arrange
        var user1Id = (await RegisterAndAuthenticateAsync()).User.Id;
        var user2Id = (await RegisterAndAuthenticateAsync()).User.Id;
        var user1Portfolio = await TestDataBuilder.CreatePortfolio(Context, user1Id);

        // ACT - User2 tries to delete User1's portfolio
        var response = await Client.DeleteAsync($"/api/users/{user2Id}/portfolios/{user1Portfolio.Id}");

        // Assert - Should not find it (authorization check)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify NOT deleted (bypass cache)
        var portfolioInDb = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == user1Portfolio.Id);

        portfolioInDb.Should().NotBeNull();
    }

    #endregion

    #region POST /api/users/{userId}/portfolios/{portfolioId}/set-default - Set As Default

    [Fact]
    public async Task SetAsDefault_UnsetsOtherDefaults()
    {
        // Arrange
        var userId = (await RegisterAndAuthenticateAsync()).User.Id;
        var retirementPortfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Retirement", isDefault: false);
        var savingsPortfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Savings", isDefault: true);

        // Act - Set portfolio2 as default
        var response = await Client.PostAsync(
            $"/api/users/{userId}/portfolios/{retirementPortfolio.Id}/set-default",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify retirementPortfolio is now default (bypass cache)
        var retirementPortfolioInDatabase = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == retirementPortfolio.Id);
        retirementPortfolioInDatabase!.IsDefault.Should().BeTrue();

        // Verify savingsPortfolio is no longer default (bypass cache)
        var portfolio1InDb = await Context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == savingsPortfolio.Id);
        portfolio1InDb!.IsDefault.Should().BeFalse();
    }

    #endregion
}