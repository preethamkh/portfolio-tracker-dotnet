using FluentAssertions;
using Moq;
using PortfolioTracker.Core.DTOs.Transaction;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Enums;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Services;

namespace PortfolioTracker.UnitTests.Services;

/// <summary>
/// Unit tests for TransactionService.
/// Tests transaction creation, updates, and automatic holding position calculations.
/// </summary>
public class TransactionServiceTests : TestBase
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IHoldingRepository> _mockHoldingRepository;
    private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockHoldingRepository = new Mock<IHoldingRepository>();
        _mockPortfolioRepository = new Mock<IPortfolioRepository>();

        _transactionService = new TransactionService(
            _mockTransactionRepository.Object,
            _mockHoldingRepository.Object,
            _mockPortfolioRepository.Object,
            CreateMockLogger<TransactionService>());
    }

    #region CreateTransactionAsync - Buy Tests

    [Fact]
    public async Task CreateTransactionAsync_BuyTransaction_ShouldIncreaseSharesAndUpdateAverageCost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();

        var createDto = new CreateTransactionDto
        {
            HoldingId = holdingId,
            TransactionType = TransactionType.Buy,
            Shares = 5,
            PricePerShare = 185m,
            Fees = 2.50m,
            TransactionDate = DateTime.UtcNow
        };

        var holding = new Holding
        {
            Id = holdingId,
            PortfolioId = portfolioId,
            Security = new Security { Symbol = "AAPL", Name = "Apple Inc.", SecurityType = "Stock" },
            TotalShares = 10,
            AverageCost = 180m // Current: 10 shares @ $180 = $1800
        };

        var portfolio = new Portfolio { Id = portfolioId, UserId = userId };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(holdingId))
            .ReturnsAsync(holding);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(portfolio);

        _mockTransactionRepository
            .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        _mockTransactionRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Transaction
            {
                Id = id,
                HoldingId = holdingId,
                Holding = holding,
                TransactionType = TransactionType.Buy,
                Shares = 5,
                PricePerShare = 185m
            });

        // Act
        var result = await _transactionService.CreateTransactionAsync(userId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.TransactionType.Should().Be(TransactionType.Buy);
        result.Shares.Should().Be(5);

        // Verify holding was updated
        // New: 5 shares @ $185 + $2.50 fees = $927.50
        // Total: $1800 + $927.50 = $2727.50 / 15 shares = $181.83 avg
        _mockHoldingRepository.Verify(
            r => r.UpdateAsync(It.Is<Holding>(h =>
                h.TotalShares == 15 &&
                Math.Abs(h.AverageCost!.Value - 181.83m) < 0.01m)),
            Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_FirstBuyTransaction_ShouldSetCorrectAverageCost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Buy,
            Shares = 10,
            PricePerShare = 180m,
            Fees = 5m,
            TransactionDate = DateTime.UtcNow
        };

        var holding = new Holding
        {
            Id = createDto.HoldingId,
            PortfolioId = Guid.NewGuid(),
            Security = new Security { Symbol = "AAPL" },
            TotalShares = 0,
            AverageCost = null // No previous purchases
        };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(createDto.HoldingId))
            .ReturnsAsync(holding);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(holding.PortfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = holding.PortfolioId, UserId = userId });

        _mockTransactionRepository
            .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        _mockTransactionRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Transaction { Holding = holding });

        // Act
        await _transactionService.CreateTransactionAsync(userId, createDto);

        // Assert
        // Average cost = (10 × $180 + $5) / 10 = $180.50
        _mockHoldingRepository.Verify(
            r => r.UpdateAsync(It.Is<Holding>(h =>
                h.TotalShares == 10 &&
                h.AverageCost == 180.50m)),
            Times.Once);
    }

    #endregion

    #region CreateTransactionAsync - Sell Tests

    [Fact]
    public async Task CreateTransactionAsync_SellTransaction_ShouldDecreaseSharesButKeepAverageCost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Sell,
            Shares = 3,
            PricePerShare = 190m,
            Fees = 2.50m,
            TransactionDate = DateTime.UtcNow
        };

        var holding = new Holding
        {
            Id = createDto.HoldingId,
            PortfolioId = Guid.NewGuid(),
            Security = new Security { Symbol = "AAPL" },
            TotalShares = 10,
            AverageCost = 180m
        };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(createDto.HoldingId))
            .ReturnsAsync(holding);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(holding.PortfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = holding.PortfolioId, UserId = userId });

        _mockTransactionRepository
            .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        _mockTransactionRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Transaction { Holding = holding });

        // Act
        await _transactionService.CreateTransactionAsync(userId, createDto);

        // Assert
        // Shares reduced to 7, but average cost stays at $180
        _mockHoldingRepository.Verify(
            r => r.UpdateAsync(It.Is<Holding>(h =>
                h.TotalShares == 7 &&
                h.AverageCost == 180m)),
            Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_SellAllShares_ShouldResetAverageCost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Sell,
            Shares = 10,
            PricePerShare = 190m,
            Fees = 0,
            TransactionDate = DateTime.UtcNow
        };

        var holding = new Holding
        {
            Id = createDto.HoldingId,
            PortfolioId = Guid.NewGuid(),
            Security = new Security { Symbol = "AAPL" },
            TotalShares = 10,
            AverageCost = 180m
        };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(createDto.HoldingId))
            .ReturnsAsync(holding);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(holding.PortfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = holding.PortfolioId, UserId = userId });

        _mockTransactionRepository
            .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        _mockTransactionRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Transaction { Holding = holding });

        // Act
        await _transactionService.CreateTransactionAsync(userId, createDto);

        // Assert
        // All shares sold, average cost should be null
        _mockHoldingRepository.Verify(
            r => r.UpdateAsync(It.Is<Holding>(h =>
                h.TotalShares == 0 &&
                h.AverageCost == null)),
            Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_SellMoreSharesThanOwned_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Sell,
            Shares = 15,
            PricePerShare = 190m,
            Fees = 0,
            TransactionDate = DateTime.UtcNow
        };

        var holding = new Holding
        {
            Id = createDto.HoldingId,
            PortfolioId = Guid.NewGuid(),
            Security = new Security { Symbol = "AAPL" },
            TotalShares = 10
        };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(createDto.HoldingId))
            .ReturnsAsync(holding);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(holding.PortfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = holding.PortfolioId, UserId = userId });

        // Act & Assert
        Func<Task> createTransactionAsyncAction = async () =>
            await _transactionService.CreateTransactionAsync(userId, createDto);

        await createTransactionAsyncAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot sell*");
    }

    #endregion

    #region CreateTransactionAsync - Validation Tests

    [Fact]
    public async Task CreateTransactionAsync_InvalidTransactionType_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Invalid,
            Shares = 10,
            PricePerShare = 180m
        };

        // Act & Assert
        Func<Task> createTransactionAsyncAction = async () =>
            await _transactionService.CreateTransactionAsync(Guid.NewGuid(), createDto);

        await createTransactionAsyncAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Transaction type must be Buy or Sell.");
    }

    [Fact]
    public async Task CreateTransactionAsync_HoldingNotFound_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Buy,
            Shares = 10,
            PricePerShare = 180m
        };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(createDto.HoldingId))
            .ReturnsAsync((Holding?)null);

        // Act & Assert
        Func<Task> createTransactionAsyncAction = async () =>
            await _transactionService.CreateTransactionAsync(Guid.NewGuid(), createDto);

        await createTransactionAsyncAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Holding {createDto.HoldingId} not found");
    }

    [Fact]
    public async Task CreateTransactionAsync_UserDoesNotOwnPortfolio_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateTransactionDto
        {
            HoldingId = Guid.NewGuid(),
            TransactionType = TransactionType.Buy,
            Shares = 10,
            PricePerShare = 180m
        };

        var holding = new Holding
        {
            Id = createDto.HoldingId,
            PortfolioId = Guid.NewGuid(),
            Security = new Security { Symbol = "AAPL" }
        };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(createDto.HoldingId))
            .ReturnsAsync(holding);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(holding.PortfolioId, userId))
            .ReturnsAsync((Portfolio?)null);

        // Act & Assert
        Func<Task> createTransactionAsyncAction = async () =>
            await _transactionService.CreateTransactionAsync(userId, createDto);

        await createTransactionAsyncAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User does not have access to this portfolio");
    }

    #endregion

    #region UpdateTransactionAsync Tests

    [Fact]
    public async Task UpdateTransactionAsync_ShouldRecalculateHolding()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var updateDto = new UpdateTransactionDto
        {
            Shares = 8, // Changed from 5 to 8
            PricePerShare = 182m,
            Fees = 3m,
            TransactionDate = DateTime.UtcNow
        };

        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            Security = new Security { Symbol = "AAPL" },
            TotalShares = 15, // 10 original + 5 from transaction
            AverageCost = 181m
        };

        var transaction = new Transaction
        {
            Id = transactionId,
            HoldingId = holding.Id,
            Holding = holding,
            TransactionType = TransactionType.Buy,
            Shares = 5, // Original value
            PricePerShare = 185m,
            Fees = 2.50m
        };

        _mockTransactionRepository
            .Setup(r => r.GetByIdWithDetailsAsync(transactionId))
            .ReturnsAsync(transaction);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(holding.PortfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = holding.PortfolioId, UserId = userId });

        // Act
        var result = await _transactionService.UpdateTransactionAsync(transactionId, userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Shares.Should().Be(8);

        // Verify transaction was updated
        _mockTransactionRepository.Verify(
            r => r.UpdateAsync(It.Is<Transaction>(t =>
                t.Shares == 8 &&
                t.PricePerShare == 182m)),
            Times.Once);

        // Verify holding was updated twice (reverse old, apply new)
        _mockHoldingRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Holding>()),
            Times.Exactly(2));
    }

    #endregion

    #region DeleteTransactionAsync Tests

    [Fact]
    public async Task DeleteTransactionAsync_ShouldReverseTransactionEffect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            Security = new Security { Symbol = "AAPL" },
            TotalShares = 15,
            AverageCost = 181m
        };

        var transaction = new Transaction
        {
            Id = transactionId,
            HoldingId = holding.Id,
            Holding = holding,
            TransactionType = TransactionType.Buy,
            Shares = 5,
            PricePerShare = 185m,
            Fees = 2.50m
        };

        _mockTransactionRepository
            .Setup(r => r.GetByIdWithDetailsAsync(transactionId))
            .ReturnsAsync(transaction);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(holding.PortfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = holding.PortfolioId, UserId = userId });

        // Act
        var result = await _transactionService.DeleteTransactionAsync(transactionId, userId);

        // Assert
        result.Should().BeTrue();

        // Verify holding was updated (reversed transaction)
        _mockHoldingRepository.Verify(
            r => r.UpdateAsync(It.Is<Holding>(h => h.TotalShares == 10)),
            Times.Once);

        _mockTransactionRepository.Verify(
            r => r.DeleteAsync(It.Is<Transaction>(t => t.Id == transactionId)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTransactionAsync_WhenNotFound_ShouldReturnFalse()
    {
        // Arrange
        _mockTransactionRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _transactionService.DeleteTransactionAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetTransactionsAsync Tests

    [Fact]
    public async Task GetHoldingTransactionsAsync_WhenUserOwnsHolding_ShouldReturnTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var holding = new Holding
        {
            Id = holdingId,
            PortfolioId = portfolioId,
            Security = new Security { Symbol = "AAPL", Name = "Apple Inc.", SecurityType = "Stock" }
        };

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = holdingId,
                Holding = holding,
                TransactionType = TransactionType.Buy,
                Shares = 10,
                PricePerShare = 180m
            }
        };

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(holdingId))
            .ReturnsAsync(holding);

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockTransactionRepository
            .Setup(r => r.GetByHoldingIdAsync(holdingId))
            .ReturnsAsync(transactions);

        // Act
        var result = (await _transactionService.GetHoldingTransactionsAsync(holdingId, userId)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPortfolioTransactionsAsync_WhenUserOwnsPortfolio_ShouldReturnAllTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Holding = new Holding
                {
                    Security = new Security { Symbol = "AAPL", Name = "Apple", SecurityType = "Stock" }
                },
                TransactionType = TransactionType.Buy,
                Shares = 10
            }
        };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockTransactionRepository
            .Setup(r => r.GetByPortfolioIdAsync(portfolioId))
            .ReturnsAsync(transactions);

        // Act
        var result = await _transactionService.GetPortfolioTransactionsAsync(portfolioId, userId);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
