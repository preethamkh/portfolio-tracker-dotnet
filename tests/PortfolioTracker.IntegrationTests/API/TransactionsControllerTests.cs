using FluentAssertions;
using PortfolioTracker.Core.DTOs.Transaction;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Enums;
using PortfolioTracker.Core.Helpers;
using PortfolioTracker.IntegrationTests.Fixtures;
using PortfolioTracker.IntegrationTests.Helpers;
using System.Net;

namespace PortfolioTracker.IntegrationTests.API;

/// <summary>
/// Integration tests for TransactionsController.
/// Tests buy/sell transaction operations and automatic holding position updates.
/// </summary>
public class TransactionsControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetPortfolioTransactions_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        ClearAuthentication();
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios/{portfolioId}/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPortfolioTransactions_WhenUserOwnsPortfolio_ShouldReturnTransactions()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", 
            "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "AAPL", "Apple Inc.");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 10,
            AverageCost = 180m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Context.Holdings.AddAsync(holding);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = holding.Id,
            TransactionType = TransactionType.Buy,
            Shares = 10,
            PricePerShare = 180m,
            TotalAmount = 1800m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow
        };

        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/portfolios/{portfolio.Id}/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.ReadAsJsonAsync<List<TransactionDto>>();

        transactions.Should().NotBeNull();
        transactions.Should().HaveCount(1);
        transactions[0].Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task GetHoldingTransactions_WhenUserOwnsHolding_ShouldReturnTransactions()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context,"MSFT", "Microsoft");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Context.Holdings.AddAsync(holding);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = holding.Id,
            TransactionType = TransactionType.Buy,
            Shares = 5,
            PricePerShare = 380m,
            TotalAmount = 1900m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/holdings/{holding.Id}/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.ReadAsJsonAsync<List<TransactionDto>>();
        transactions.Should().ContainSingle();

    }

    [Fact]
    public async Task GetTransaction_WhenTransactionExists_ShouldReturnTransaction()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "AAPL", "Apple");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Context.Holdings.AddAsync(holding);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = holding.Id,
            TransactionType = TransactionType.Buy,
            Shares = 10,
            PricePerShare = 180m,
            TotalAmount = 1800m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/transactions/{transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.ReadAsJsonAsync<TransactionDto>();
        result.Should().NotBeNull();
        result.Id.Should().Be(transaction.Id);
    }

    [Fact]
    public async Task CreateTransaction_BuyTransaction_ShouldCreate201AndUpdateHolding()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "AAPL", "Apple");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 10,
            AverageCost = 180m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await Context.Holdings.AddAsync(holding);
        await Context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            HoldingId = holding.Id,
            TransactionType = TransactionType.Buy,
            Shares = 5,
            PricePerShare = 185m,
            Fees = 2.50m,
            TransactionDate = DateTime.UtcNow,
            Notes = "Additional purchase"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/transactions", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.ReadAsJsonAsync<TransactionDto>();
        result.Should().NotBeNull();
        result.TransactionType.Should().Be(TransactionType.Buy);
        result.Shares.Should().Be(5);

        // Clearing to avoid the detached entity problem - test's DbContext is not aware of changes made by the API's DbContext
        // Clear tracking to get fresh data
        Context.ChangeTracker.Clear();

        // Verify holding was updated
        var updatedHolding = await Context.Holdings.FindAsync(holding.Id);
        updatedHolding.Should().NotBeNull();
        updatedHolding.TotalShares.Should().Be(15); // 10 + 5
        updatedHolding.AverageCost.Should().NotBeNull();
        // New avg = (10 × 180 + 5 × 185 + 2.50) / 15 = 181.83
        Math.Abs(updatedHolding.AverageCost!.Value - 181.83m).Should().BeLessThan(0.01m);
    }

    [Fact]
    public async Task CreateTransaction_SellTransaction_ShouldCreate201AndReduceShares()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "MSFT", "Microsoft");


        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 10,
            AverageCost = 380m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Context.Holdings.AddAsync(holding);
        await Context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            HoldingId = holding.Id,
            TransactionType = TransactionType.Sell,
            Shares = 3,
            PricePerShare = 390m,
            Fees = 2.50m,
            TransactionDate = DateTime.UtcNow,
            Notes = "Partial sell"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/transactions", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.ReadAsJsonAsync<TransactionDto>();
        result!.TransactionType.Should().Be(TransactionType.Sell);

        // Clear tracking to get fresh data
        Context.ChangeTracker.Clear();

        // Verify holding was updated
        var updatedHolding = await Context.Holdings.FindAsync(holding.Id);
        updatedHolding.Should().NotBeNull();
        updatedHolding.TotalShares.Should().Be(7); // 10 - 3
        updatedHolding.AverageCost.Should().Be(380m); // Unchanged on sell
    }

    [Fact]
    public async Task CreateTransaction_SellAllShares_ShouldResetAverageCost()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "GOOGL", "Alphabet");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 10,
            AverageCost = 142m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await Context.Holdings.AddAsync(holding);
        await Context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            HoldingId = holding.Id,
            TransactionType = TransactionType.Sell,
            Shares = 10,
            PricePerShare = 150m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/transactions", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Clear tracking to get fresh data
        Context.ChangeTracker.Clear();

        // Verify holding was updated
        var updatedHolding = await Context.Holdings.FindAsync(holding.Id);
        updatedHolding.Should().NotBeNull();
        updatedHolding.TotalShares.Should().Be(0);
        updatedHolding.AverageCost.Should().BeNull(); // Reset when all sold
    }

    [Fact]
    public async Task CreateTransaction_SellMoreThanOwned_ShouldReturn400()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "AAPL", "Apple");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Context.Holdings.AddAsync(holding);
        await Context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            HoldingId = holding.Id,
            TransactionType = TransactionType.Sell,
            Shares = 10, // More than owned
            PricePerShare = 180m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/transactions", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransaction_InvalidTransactionType_ShouldReturn400()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Invalid,
            Shares = 10,
            PricePerShare = 180m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/transactions", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTransaction_WithValidData_ShouldReturn200AndRecalculateHolding()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "AAPL", "Apple");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 15, // Result of 10 initial + 5 from transaction
            AverageCost = 181.83m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Context.Holdings.AddAsync(holding);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = holding.Id,
            TransactionType = TransactionType.Buy,
            Shares = 5,
            PricePerShare = 185m,
            TotalAmount = 925m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow
        };
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        var updateDto = new UpdateTransactionDto
        {
            Shares = 8, // Change from 5 to 8
            PricePerShare = 182m,
            Fees = 3m,
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/users/{userId}/transactions/{transaction.Id}",
            updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.ReadAsJsonAsync<TransactionDto>();
        result!.Shares.Should().Be(8);

        Context.ChangeTracker.Clear();

        // Verify holding was recalculated
        var updatedHolding = await Context.Holdings.FindAsync(holding.Id);
        updatedHolding!.TotalShares.Should().Be(18); // 10 initial + 8 new
    }

    [Fact]
    public async Task DeleteTransaction_WhenTransactionExists_ShouldReturn204AndReverseEffect()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync("preetham@test.com", "Password123!");
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId, "Test Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "MSFT", "Microsoft");

        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 15,
            AverageCost = 381m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await Context.Holdings.AddAsync(holding);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = holding.Id,
            TransactionType = TransactionType.Buy,
            Shares = 5,
            PricePerShare = 385m,
            TotalAmount = 1925m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/users/{userId}/transactions/{transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Context.ChangeTracker.Clear();

        // Verify transaction is deleted
        var deletedTransaction = await Context.Transactions.FindAsync(transaction.Id);
        deletedTransaction.Should().BeNull();

        // Verify holding was updated (reversed)
        var updatedHolding = await Context.Holdings.FindAsync(holding.Id);
        updatedHolding!.TotalShares.Should().Be(10); // 15 - 5
    }

    [Fact]
    public async Task DeleteTransaction_WhenUserDoesNotOwnPortfolio_ShouldReturn403()
    {
        // Arrange
        var user1 = await TestDataBuilder.CreateUser(Context, "user1@example.com");
        
        var portfolio = await TestDataBuilder.CreatePortfolio(Context, user1.Id, "User1 Portfolio");
        var security = await TestDataBuilder.CreateSecurity(Context, "AAPL", "Apple");
        
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            TotalShares = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Context.Holdings.AddAsync(holding);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = holding.Id,
            TransactionType = TransactionType.Buy,
            Shares = 10,
            PricePerShare = 180m,
            TotalAmount = 1800m,
            Fees = 0m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        // Act - user2 tries to delete user1's transaction
        await RegisterAndAuthenticateAsync("user2@example.com", "Password123!");
        var response = await Client.DeleteAsync($"/api/users/{user1.Id}/transactions/{transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
