using FluentAssertions;
using Moq;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Core.Services;

namespace PortfolioTracker.UnitTests.Services;

public class SecurityServiceTests : TestBase
{
    private readonly Mock<ISecurityRepository> _mockSecurityRepository;
    private readonly Mock<IStockDataService> _mockStockDataService;
    private readonly ISecurityService _securityService;

    public SecurityServiceTests()
    {
        _mockSecurityRepository = new Mock<ISecurityRepository>();
        _mockStockDataService = new Mock<IStockDataService>();
        _securityService = new SecurityService(
            _mockSecurityRepository.Object,
            _mockStockDataService.Object,
            CreateMockLogger<SecurityService>()
        );
    }

    [Fact]
    public async Task SearchSecuritiesAsync_WhenSecuritiesFoundInExternal_ShouldReturnResults()
    {
        // Arrange
        var query = "apple";
        var externalResult = new List<ExternalSecuritySearchDto>
        {
            new() { Symbol = "AAPL", Name = "Apple Inc.", Type = "Stock", Currency = "USD" },
            new() { Symbol = "APLE", Name = "Apple Hospitality REIT Inc.", Type = "Stock", Currency = "USD" }
        };

        _mockStockDataService
            .Setup(s => s.SearchSecuritiesAsync(query, It.IsAny<int>()))
            .ReturnsAsync(externalResult);

        // simulates that the security does not exist in our DB yet
        _mockSecurityRepository
            .Setup(r => r.GetBySymbolAsync(It.IsAny<string>()))
            .ReturnsAsync((Security?)null);

        // Act
        var results = await _securityService.SearchSecuritiesAsync(query);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.First().Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task SearchSecuritiesAsync_WhenSecurityExistsInDatabase_ShouldReturnDatabaseVersion()
    {
        // Arrange
        var query = "apple";
        var externalResult = new List<ExternalSecuritySearchDto>
        {
            new() { Symbol = "AAPL", Name = "Apple Inc.", Type = "Stock", Currency = "USD" }
        };

        var securityInDatabase = new Security
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Name = "Apple Inc.",
            SecurityType = "Stock",
            Currency = "USD"
        };

        _mockStockDataService
            .Setup(s => s.SearchSecuritiesAsync(query, It.IsAny<int>()))
            .ReturnsAsync(externalResult);

        _mockSecurityRepository
            .Setup(r => r.GetBySymbolAsync(It.IsAny<string>()))
            .ReturnsAsync(securityInDatabase);

        // Act
        var results = await _securityService.SearchSecuritiesAsync(query);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.First().Symbol.Should().Be("AAPL");
        results.First().Id.Should().Be(securityInDatabase.Id);
    }

    [Fact]
    public async Task GetSecurityByIdAsync_WhenSecurityExists_ShouldReturnSecurity()
    {
        // Arrange
        var securityId = Guid.NewGuid();
        var security = new Security
        {
            Id = securityId,
            Symbol = "AAPL",
            Name = "Apple Inc.",
            SecurityType = "Stock",
            Currency = "USD"
        };

        _mockSecurityRepository
            .Setup(r => r.GetByIdAsync(securityId))
            .ReturnsAsync(security);

        // Act
        var result = await _securityService.GetSecurityByIdAsync(securityId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(securityId);
        result.Symbol.Should().Be("AAPL");
    }
    [Fact]
    public async Task GetSecurityByIdAsync_WhenSecurityDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var securityId = Guid.NewGuid();
        _mockSecurityRepository
            .Setup(r => r.GetByIdAsync(securityId))
            .ReturnsAsync((Security?)null);

        // Act
        var result = await _securityService.GetSecurityByIdAsync(securityId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSecurityBySymbolAsync_WhenSecurityExists_ShouldReturnSecurity()
    {
        // Arrange
        var security = new Security
        {
            Id = Guid.NewGuid(),
            Symbol = "MSFT",
            Name = "Microsoft Corporation",
            SecurityType = "Stock"
        };

        _mockSecurityRepository
            .Setup(r => r.GetBySymbolAsync("MSFT"))
            .ReturnsAsync(security);

        // Act
        var result = await _securityService.GetSecurityBySymbolAsync("MSFT");

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("MSFT");
    }

    [Fact]
    public async Task GetOrCreateSecurityAsync_WhenSecurityExists_ShouldReturnExisting()
    {
        // Arrange
        var securityId = Guid.NewGuid();
        var existingSecurity = new Security
        {
            Id = securityId,
            Symbol = "AAPL",
            Name = "Apple Inc.",
            SecurityType = "Stock"
        };

        _mockSecurityRepository
            .Setup(r => r.GetBySymbolAsync("AAPL"))
            .ReturnsAsync(existingSecurity);

        // Act
        var result = await _securityService.GetOrCreateSecurityAsync("AAPL");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(securityId);

        // Verify we didn't try to create
        _mockSecurityRepository.Verify(
            r => r.AddAsync(It.IsAny<Security>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOrCreateSecurityAsync_WhenSecurityDoesNotExist_ShouldCreateNew()
    {
        // Arrange
        var companyInfo = new CompanyInfoDto
        {
            Symbol = "TSLA",
            Name = "Tesla, Inc.",
            Exchange = "NASDAQ",
            Sector = "Consumer Cyclical",
            Industry = "Auto Manufacturers",
            Currency = "USD"
        };

        _mockSecurityRepository
            .Setup(r => r.GetBySymbolAsync("TSLA"))
            .ReturnsAsync((Security?)null);

        _mockStockDataService
            .Setup(s => s.GetCompanyInfoAsync("TSLA"))
            .ReturnsAsync(companyInfo);

        _mockSecurityRepository
            .Setup(r => r.AddAsync(It.IsAny<Security>()))
            .ReturnsAsync((Security s) => s);
        _mockSecurityRepository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _securityService.GetOrCreateSecurityAsync("TSLA");

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("TSLA");
        result.Name.Should().Be("Tesla, Inc.");

        // Verify security was created
        _mockSecurityRepository.Verify(
            r => r.AddAsync(It.Is<Security>(s => s.Symbol == "TSLA" && s.Name == "Tesla, Inc.")),
            Times.Once);

        _mockSecurityRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateSecurityAsync_WhenAPIReturnsNull_ShouldThrowException()
    {
        // Arrange
        _mockSecurityRepository
            .Setup(r => r.GetBySymbolAsync("UNKNOWN"))
            .ReturnsAsync((Security?)null);

        _mockStockDataService
            .Setup(s => s.GetCompanyInfoAsync("INVALID"))
            .ReturnsAsync((CompanyInfoDto?) null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _securityService.GetOrCreateSecurityAsync("INVALID");
        });

        // Verify no security was created
        _mockSecurityRepository.Verify(
            r => r.AddAsync(It.IsAny<Security>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOrCreateSecurityAsync_WithETFName_ShouldSetSecurityTypeToETF()
    {
        // Arrange
        var companyInfo = new CompanyInfoDto
        {
            Symbol = "SPY",
            Name = "SPDR S&P 500 ETF Trust",
            Exchange = "NYSE",
            Currency = "USD"
        };

        _mockSecurityRepository
            .Setup(r => r.GetBySymbolAsync("SPY"))
            .ReturnsAsync((Security?)null);

        _mockStockDataService
            .Setup(s => s.GetCompanyInfoAsync("SPY"))
            .ReturnsAsync(companyInfo);

        _mockSecurityRepository
            .Setup(r => r.AddAsync(It.IsAny<Security>()))
            .ReturnsAsync((Security s) => s);

        // Act
        var result = await _securityService.GetOrCreateSecurityAsync("SPY");

        // Assert
        result.Should().NotBeNull();
        result.SecurityType.Should().Be("ETF");

        // Verify security type was set correctly
        _mockSecurityRepository.Verify(
            r => r.AddAsync(It.Is<Security>(s => s.SecurityType == "ETF")),
            Times.Once);
    }
}
