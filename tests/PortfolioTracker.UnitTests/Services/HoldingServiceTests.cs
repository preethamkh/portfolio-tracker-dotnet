using FluentAssertions;
using Moq;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.DTOs.Holding;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Core.Services;

namespace PortfolioTracker.UnitTests.Services;

/// <summary>
/// Unit tests for HoldingService
/// Tests business logic for holdings management including validation and price enrichment.
/// </summary>
public class HoldingServiceTests : TestBase
{
    private readonly Mock<IHoldingRepository> _mockHoldingRepository;
    private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
    private readonly Mock<ISecurityRepository> _mockSecurityRepository;
    private readonly Mock<IStockDataService> _mockStockDataService;
    private readonly HoldingService _holdingService;

    public HoldingServiceTests()
    {
        _mockHoldingRepository = new Mock<IHoldingRepository>();
        _mockPortfolioRepository = new Mock<IPortfolioRepository>();
        _mockSecurityRepository = new Mock<ISecurityRepository>();
        _mockStockDataService = new Mock<IStockDataService>();

        _holdingService = new HoldingService(
            _mockHoldingRepository.Object,
            _mockPortfolioRepository.Object,
            _mockSecurityRepository.Object,
            _mockStockDataService.Object,
            CreateMockLogger<HoldingService>());
    }

    #region GetPortfolioHoldingsAsync Tests

    [Fact]
    public async Task GetPortfolioHoldingAsync_WhenUserDoesNotOwnPortfolio_ShouldReturnEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync((Portfolio?)null);

        // Act
        var result = await _holdingService.GetPortfolioHoldingsAsync(portfolioId, userId);

        // Assert
        result.Should().BeEmpty();

        _mockHoldingRepository
            .Verify(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never());
    }

    [Fact]
    public async Task GetPortfolioHoldingsAsync_WhenUserOwnsPortfolio_ShouldReturnHoldings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var securityId = Guid.NewGuid();

        var portfolio = new Portfolio
        {
            Id = portfolioId,
            UserId = userId,
            Name = "Test Portfolio"
        };

        var security = new Security
        {
            Id = securityId,
            Symbol = "AAPL",
            Name = "Apple Inc.",
            SecurityType = "Stock"
        };

        var holdings = new List<Holding>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                SecurityId = securityId,
                Security = security,
                TotalShares = 10,
                AverageCost = 180.50m
            }
        };

        var quote = new StockQuoteDto
        {
            Symbol = "AAPL",
            Price = 185.75m
        };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(portfolio);

        _mockHoldingRepository
            .Setup(r => r.GetByPortfolioIdAsync(portfolioId))
            .ReturnsAsync(holdings);

        _mockStockDataService
            .Setup(s => s.GetQuoteAsync("AAPL"))
            .ReturnsAsync(quote);

        // Act
        var result = (await _holdingService.GetPortfolioHoldingsAsync(portfolioId, userId)).ToList();

        // Assert
        var holding = result.First();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        holding.Symbol.Should().Be("AAPL");
        holding.CurrentPrice.Should().Be(185.75m);
        holding.CurrentValue.Should().Be(1857.50m); // 10 shares × $185.75
        holding.TotalCost.Should().Be(1805.00m); // 10 shares × $180.50
        holding.UnrealizedGainLoss.Should().Be(52.50m); // $1857.50 - $1805
        holding.UnrealizedGainLossPercent.Should().BeApproximately(2.91m, 0.01m); // (52.50 / 1805) × 100
    }

    [Fact]
    public async Task GetPortfolioHoldingsAsync_WhenPriceServiceFails_ShouldReturnHoldingsWithoutPrices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdings = new List<Holding>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                Security = new Security { Symbol = "AAPL", Name = "Apple Inc.", SecurityType = "Stock" },
                TotalShares = 10
            }
        };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockHoldingRepository
            .Setup(r => r.GetByPortfolioIdAsync(portfolioId))
            .ReturnsAsync(holdings);

        _mockStockDataService
            .Setup(s => s.GetQuoteAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = (await _holdingService.GetPortfolioHoldingsAsync(portfolioId, userId)).ToList();

        // Assert
        result.Should().NotBeEmpty();
        var holding = result.First();
        holding.CurrentPrice.Should().BeNull();
        holding.CurrentValue.Should().BeNull();
    }

    #endregion

    #region GetHoldingByIdAsync Tests

    [Fact]
    public async Task GetHoldingByIdAsync_WhenHoldingExists_ShouldReturnHolding()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();

        var portfolio = new Portfolio { Id = portfolioId, UserId = userId };
        var holding = new Holding
        {
            Id = holdingId,
            PortfolioId = portfolioId,
            Security = new Security { Symbol = "MSFT", Name = "Microsoft", SecurityType = "Stock" },
            TotalShares = 5
        };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(portfolio);

        _mockHoldingRepository
            .Setup(r => r.ExistsInPortfolioAsync(holdingId, portfolioId))
            .ReturnsAsync(true);

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(holdingId))
            .ReturnsAsync(holding);

        _mockStockDataService
            .Setup(s => s.GetQuoteAsync("MSFT"))
            .ReturnsAsync(new StockQuoteDto { Symbol = "MSFT", Price = 380m });

        // Act
        var result = await _holdingService.GetHoldingByIdAsync(holdingId, portfolioId, userId);

        // Assert
        result.Should().NotBeNull();
        result.HoldingId.Should().Be(holdingId);
        result.Symbol.Should().Be("MSFT");
    }

    [Fact]
    public async Task GetHoldingByIdAsync_WhenHoldingNotInPortfolio_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockHoldingRepository
            .Setup(r => r.ExistsInPortfolioAsync(holdingId, portfolioId))
            .ReturnsAsync(false);

        // Act
        var result = await _holdingService.GetHoldingByIdAsync(holdingId, portfolioId, userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateHoldingAsync Tests

    [Fact]
    public async Task CreateHoldingAsync_WithValidData_ShouldCreateHolding()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var securityId = Guid.NewGuid();

        var createDto = new CreateHoldingDto
        {
            SecurityId = securityId,
            TotalShares = 10,
            AverageCost = 180.50m
        };

        var portfolio = new Portfolio { Id = portfolioId, UserId = userId };
        var security = new Security
        {
            Id = securityId,
            Symbol = "AAPL",
            Name = "Apple Inc.",
            SecurityType = "Stock"
        };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(portfolio);

        _mockSecurityRepository
            .Setup(r => r.GetByIdAsync(securityId))
            .ReturnsAsync(security);

        _mockHoldingRepository
            .Setup(r => r.GetByPortfolioAndSecurityAsync(portfolioId, securityId))
            .ReturnsAsync((Holding?)null);

        _mockHoldingRepository
            .Setup(r => r.AddAsync(It.IsAny<Holding>()))
            .ReturnsAsync((Holding h) => h);

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Holding
            {
                Id = id,
                PortfolioId = portfolioId,
                SecurityId = securityId,
                Security = security,
                TotalShares = 10,
                AverageCost = 180.50m
            });

        _mockStockDataService
            .Setup(s => s.GetQuoteAsync("AAPL"))
            .ReturnsAsync(new StockQuoteDto { Symbol = "AAPL", Price = 185m });

        // Act
        var result = await _holdingService.CreateHoldingAsync(portfolioId, userId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.TotalShares.Should().Be(10);
        result.AverageCost.Should().Be(180.50m);

        _mockHoldingRepository.Verify(
            r => r.AddAsync(It.Is<Holding>(h =>
                h.SecurityId == securityId &&
                h.TotalShares == 10)),
            Times.Once);
    }

    [Fact]
    public async Task CreateHoldingAsync_WhenPortfolioNotFound_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var createHoldingDto = new CreateHoldingDto { SecurityId = Guid.NewGuid() };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync((Portfolio?)null);

        // Act & Assert
        // We do not await the method directly, because we want to capture the exception for assertion (else it would throw before we can assert)
        Func<Task> createHoldingAsyncAction = async () =>
            await _holdingService.CreateHoldingAsync(portfolioId, userId, createHoldingDto);

        await createHoldingAsyncAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Portfolio {portfolioId} not found or user does not have access");
    }

    [Fact]
    public async Task CreateHoldingAsync_WhenSecurityNotFound_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var securityId = Guid.NewGuid();
        var createHoldingDto = new CreateHoldingDto { SecurityId = securityId };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockSecurityRepository
            .Setup(r => r.GetByIdAsync(securityId))
            .ReturnsAsync((Security?)null);

        // Act & Assert
        Func<Task> createHoldingAsyncAction = async () =>
            await _holdingService.CreateHoldingAsync(portfolioId, userId, createHoldingDto);

        await createHoldingAsyncAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Security {createHoldingDto.SecurityId} not found");
    }

    [Fact]
    public async Task CreateHoldingAsync_WhenHoldingAlreadyExists_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var securityId = Guid.NewGuid();
        var createHoldingDto = new CreateHoldingDto { SecurityId = securityId };

        var existingHolding = new Holding
        {
            Id = Guid.NewGuid(),
            SecurityId = securityId
        };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockSecurityRepository
            .Setup(r => r.GetByIdAsync(securityId))
            .ReturnsAsync(new Security { Id = securityId, Symbol = "AAPL" });

        _mockHoldingRepository
            .Setup(r => r.GetByPortfolioAndSecurityAsync(portfolioId, securityId))
            .ReturnsAsync(existingHolding);

        // Act & Assert
        Func<Task> createHoldingAsyncAction = async () =>
            await _holdingService.CreateHoldingAsync(portfolioId, userId, createHoldingDto);

        await createHoldingAsyncAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Holding for security AAPL already exists in this portfolio");
    }

    #endregion

    #region UpdateHoldingAsync Tests

    [Fact]
    public async Task UpdateHoldingAsync_WithValidData_ShouldUpdateHolding()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();

        var updateDto = new UpdateHoldingDto
        {
            TotalShares = 15,
            AverageCost = 182m
        };

        var holding = new Holding
        {
            Id = holdingId,
            PortfolioId = portfolioId,
            Security = new Security { Symbol = "AAPL", Name = "Apple", SecurityType = "Stock" },
            TotalShares = 10,
            AverageCost = 180m
        };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockHoldingRepository
            .Setup(r => r.ExistsInPortfolioAsync(holdingId, portfolioId))
            .ReturnsAsync(true);

        _mockHoldingRepository
            .Setup(r => r.GetByIdWithDetailsAsync(holdingId))
            .ReturnsAsync(holding);

        _mockStockDataService
            .Setup(s => s.GetQuoteAsync("AAPL"))
            .ReturnsAsync(new StockQuoteDto { Symbol = "AAPL", Price = 185m });

        // Act
        var result = await _holdingService.UpdateHoldingAsync(holdingId, portfolioId, userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.TotalShares.Should().Be(15);
        result.AverageCost.Should().Be(182m);

        _mockHoldingRepository.Verify(
            r => r.UpdateAsync(It.Is<Holding>(h =>
                h.TotalShares == 15 &&
                h.AverageCost == 182m)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateHoldingAsync_WhenHoldingNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();
        var updateDto = new UpdateHoldingDto();

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockHoldingRepository
            .Setup(r => r.ExistsInPortfolioAsync(holdingId, portfolioId))
            .ReturnsAsync(false);

        // Act
        var result = await _holdingService.UpdateHoldingAsync(holdingId, portfolioId, userId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteHoldingAsync Tests

    [Fact]
    public async Task DeleteHoldingAsync_WhenHoldingExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();

        var holding = new Holding { Id = holdingId, PortfolioId = portfolioId };

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockHoldingRepository
            .Setup(r => r.ExistsInPortfolioAsync(holdingId, portfolioId))
            .ReturnsAsync(true);

        _mockHoldingRepository
            .Setup(r => r.GetByIdAsync(holdingId))
            .ReturnsAsync(holding);

        // Act
        var result = await _holdingService.DeleteHoldingAsync(holdingId, portfolioId, userId);

        // Assert
        result.Should().BeTrue();

        _mockHoldingRepository.Verify(
            r => r.DeleteAsync(It.Is<Holding>(h => h.Id == holdingId)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteHoldingAsync_WhenHoldingNotFound_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();

        _mockPortfolioRepository
            .Setup(r => r.GetByIdAndUserIdAsync(portfolioId, userId))
            .ReturnsAsync(new Portfolio { Id = portfolioId, UserId = userId });

        _mockHoldingRepository
            .Setup(r => r.ExistsInPortfolioAsync(holdingId, portfolioId))
            .ReturnsAsync(false);

        // Act
        var result = await _holdingService.DeleteHoldingAsync(holdingId, portfolioId, userId);

        // Assert
        result.Should().BeFalse();

        _mockHoldingRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Holding>()),
            Times.Never);
    }

    #endregion
}
