using FluentAssertions;
using Moq;
using PortfolioTracker.Core.DTOs.Portfolio;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Services;

namespace PortfolioTracker.UnitTests.Services
{
    /// <summary>
    /// Unit tests for PortfolioService.
    /// Tests business logic for portfolio management including authorization.
    /// </summary>
    /// <remarks>
    /// Key Differences from UserService Tests:
    /// 1. Authorization: Every method checks userId matches portfolio owner
    /// 2. Complex Business Rules: Default portfolio, duplicate names per user
    /// 3. Multi-entity: Tests interaction between User and Portfolio
    /// </remarks>
    public class PortfolioServiceTests : TestBase
    {
        private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly PortfolioService _portfolioService;

        public PortfolioServiceTests()
        {
            _mockPortfolioRepository = new Mock<IPortfolioRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _portfolioService = new PortfolioService(_mockPortfolioRepository.Object, _mockUserRepository.Object, CreateMockLogger<PortfolioService>());
        }

        [Fact]
        public async Task GetUserPortfoliosAsync_WhenPortfoliosExist_ShouldReturnAllUserPortfolios()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolios = new List<Portfolio>()
            {
                new Portfolio
                {
                    Id = Guid.NewGuid(),
                    Name = "Retirement",
                    UserId = userId,
                    IsDefault = true,
                    Currency = "AUD"
                },
                new Portfolio
                {
                    Id = Guid.NewGuid(),
                    Name = "Trading",
                    UserId = userId,
                    IsDefault = false,
                    Currency = "AUD"
                }
            };

            _mockPortfolioRepository
                .Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(portfolios);

            // Act
            var result = (await _portfolioService.GetUserPortfoliosAsync(userId)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Retirement");
            result.First().IsDefault.Should().BeTrue();

            _mockPortfolioRepository.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserPortfoliosAsync_WhenNoPortfolios_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockPortfolioRepository
                .Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(new List<Portfolio>());

            // Act
            var result = (await _portfolioService.GetUserPortfoliosAsync(userId)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _mockPortfolioRepository.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        }

        /// <summary>
        /// Tests authorization: User can only access their own portfolios.
        /// </summary>
        [Fact]
        public async Task GetPortfolioByIdAsync__WhenPortfolioBelongsToUser_ShouldReturnPortfolio()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var portfolio = new Portfolio
            {
                Id = portfolioId,
                Name = "Retirement",
                UserId = userId,
                IsDefault = true,
                Currency = "AUD"
            };

            // Repository enforces authorization at data layer
            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync(portfolio);

            // Act
            var result = await _portfolioService.GetPortfolioByIdAsync(portfolioId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(portfolioId);
            result.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_WhenPortfolioNotOwnedByUser_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            // Repository returns null if portfolio does not belong to user
            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _portfolioService.GetPortfolioByIdAsync(portfolioId, userId);

            // Assert
            // Authorization failed - user cannot access this portfolio
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreatePortfolioAsync_WithValidData_ShouldCreatePortfolio()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createPortfolioDto = new CreatePortfolioDto()
            {
                Name = "Index Funds",
                Description = "Long-term investment in index funds",
                Currency = "AUD",
                IsDefault = false
            };

            // User exists
            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "user1@test.com" });

            // No existing portfolio with same name for this user
            _mockPortfolioRepository
                .Setup(repo => repo.UserHasPortfolioWithNameAsync(userId, createPortfolioDto.Name, null))
                .ReturnsAsync(false);

            // simulate id generation on create
            // Whenever AddAsync is called with any Portfolio object, run this lambda.
            _mockPortfolioRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Portfolio>()))
                .ReturnsAsync((Portfolio p) =>
                {
                    p.Id = Guid.NewGuid();
                    return p;
                });

            _mockPortfolioRepository
                .Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _portfolioService.CreatePortfolioAsync(userId, createPortfolioDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Index Funds");
            result.UserId.Should().Be(userId);
            result.Currency.Should().Be("AUD");
            result.Id.Should().NotBe(Guid.Empty);

            _mockPortfolioRepository.Verify(repo => repo.AddAsync(It.IsAny<Portfolio>()), Times.Once);
        }

        /// <summary>
        /// Business Rule: Cannot create portfolio if user not found.
        /// </summary>
        [Fact]
        public async Task CreatePortfolioAsync_WhenUserNotFound_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createPortfolioDto = new CreatePortfolioDto()
            {
                Name = "Index Funds",
                Description = "Long-term investment in index funds",
                Currency = "AUD",
                IsDefault = false
            };

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> createPortfolioAsyncAction = async () =>
                await _portfolioService.CreatePortfolioAsync(userId, createPortfolioDto);

            await createPortfolioAsyncAction.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"User with ID {userId} not found");
        }

        /// <summary>
        /// Business Rule: User cannot have duplicate portfolio names.
        /// </summary>
        [Fact]
        public async Task CreatePortfolioAsync_WithDuplicateName_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createPortfolioDto = new CreatePortfolioDto()
            {
                Name = "Index Funds",
                Description = "Long-term investment in index funds",
                Currency = "AUD",
                IsDefault = false
            };

            // User exists
            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "user1@test.com" });

            // Existing portfolio with same name for this user
            _mockPortfolioRepository
                .Setup(repo => repo.UserHasPortfolioWithNameAsync(userId, createPortfolioDto.Name, null))
                .ReturnsAsync(true);

            // Act
            Func<Task> createPortfolioAsyncAction = async () => await _portfolioService.CreatePortfolioAsync(userId, createPortfolioDto);

            await createPortfolioAsyncAction.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"You already have a portfolio named '{createPortfolioDto.Name}'");

            _mockPortfolioRepository.Verify(repo => repo.AddAsync(It.IsAny<Portfolio>()), Times.Never);
            _mockPortfolioRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        /// <summary>
        /// Business Rule: Setting new portfolio as default should unset other defaults.
        /// This test verifies the SetAsDefaultAsync method is called.
        /// </summary>
        [Fact]
        public async Task CreatePortfolioAsync_WithIsDefaultTrue_ShouldSetAsDefault()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createPortfolioDto = new CreatePortfolioDto()
            {
                Name = "Index Funds",
                Description = "Long-term investment in index funds",
                Currency = "AUD",
                IsDefault = true
            };

            // User exists
            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "user1@test.com" });

            // No existing portfolio with same name for this user
            _mockPortfolioRepository
                .Setup(repo => repo.UserHasPortfolioWithNameAsync(userId, createPortfolioDto.Name, null))
                .ReturnsAsync(false);

            _mockPortfolioRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Portfolio>()))
                .ReturnsAsync((Portfolio p) =>
                {
                    p.Id = Guid.NewGuid();
                    return p;
                });

            _mockPortfolioRepository
                .Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockPortfolioRepository
                .Setup(repo => repo.SetAsDefaultAsync(It.IsAny<Guid>(), userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.CreatePortfolioAsync(userId, createPortfolioDto);

            // Assert
            result.IsDefault.Should().BeTrue();

            // Verify SetAsDefaultAsync was called to unset other defaults.
            _mockPortfolioRepository.Verify(repo => repo.SetAsDefaultAsync(It.IsAny<Guid>(), userId), Times.Once);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_WithValidData_ShouldUpdatePortfolio()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var existingPortfolio = new Portfolio
            {
                Id = portfolioId,
                UserId = userId,
                Name = "Old Name",
                Description = "Old Description",
                Currency = "AUD"
            };

            var updatePortfolioDto = new UpdatePortfolioDto
            {
                Name = "New Name",
                Description = "New Description"
            };

            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync(existingPortfolio);

            _mockPortfolioRepository
                .Setup(repo => repo.UserHasPortfolioWithNameAsync(userId, updatePortfolioDto.Name!, portfolioId))
                .ReturnsAsync(false);

            _mockPortfolioRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Portfolio>()))
                .Returns(Task.CompletedTask);

            _mockPortfolioRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _portfolioService.UpdatePortfolioAsync(portfolioId, userId, updatePortfolioDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Name");
            result.Description.Should().Be("New Description");

            _mockPortfolioRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Portfolio>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_WithDuplicateName_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var existingPortfolio = new Portfolio
            {
                Id = portfolioId,
                UserId = userId,
                Name = "Old Name",
                Description = "Old Description",
                Currency = "AUD"
            };

            var updatePortfolioDto = new UpdatePortfolioDto
            {
                Name = "Taken Name",
                Description = "Taken Description"
            };

            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync(existingPortfolio);

            _mockPortfolioRepository
                .Setup(repo => repo.UserHasPortfolioWithNameAsync(userId, updatePortfolioDto.Name!, portfolioId))
                .ReturnsAsync(true);

            // Act
            Func<Task> updatePortfolioAsyncAction = async () => await _portfolioService.UpdatePortfolioAsync(portfolioId, userId, updatePortfolioDto);

            // Assert
            await updatePortfolioAsyncAction.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"You already have another portfolio named 'Taken Name'");

            _mockPortfolioRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Portfolio>()),
                Times.Never);
        }

        /// <summary>
        /// Tests authorization during update.
        /// </summary>
        [Fact]
        public async Task UpdatePortfolioAsync_WhenNotAuthorized_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var updateDto = new UpdatePortfolioDto { Name = "New Name" };

            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _portfolioService.UpdatePortfolioAsync(portfolioId, userId, updateDto);

            // Assert
            result.Should().BeNull();

            _mockPortfolioRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Portfolio>()),
                Times.Never
            );
        }

        [Fact]
        public async Task DeletePortfolioAsync_WhenAuthorized_ShouldDeletePortfolio()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var portfolioToDelete = new Portfolio
            {
                Id = portfolioId,
                UserId = userId,
                Name = "To Delete"
            };

            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync(portfolioToDelete);

            _mockPortfolioRepository
                .Setup(repo => repo.DeleteAsync(portfolioToDelete))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.DeletePortfolioAsync(portfolioId, userId);

            // Assert
            result.Should().BeTrue();

            _mockPortfolioRepository.Verify(
                repo => repo.DeleteAsync(portfolioToDelete),
                Times.Once);
        }

        [Fact]
        public async Task DeletePortfolioAsync_WhenNotAuthorized_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _portfolioService.DeletePortfolioAsync(portfolioId, userId);

            // Assert
            result.Should().BeFalse();

            _mockPortfolioRepository.Verify(
                repo => repo.DeleteAsync(It.IsAny<Portfolio>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SetAsDefaultAsync_WhenNotAuthorized_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            _mockPortfolioRepository
                .Setup(repo => repo.GetByIdAndUserIdAsync(portfolioId, userId))
                .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _portfolioService.SetAsDefaultAsync(portfolioId, userId);

            // Assert
            result.Should().BeFalse();

            _mockPortfolioRepository.Verify(
                repo => repo.SetAsDefaultAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Never
            );
        }

        [Fact]
        public async Task GetDefaultPortfolioAsync_WhenDefaultExists_ShouldReturnDefaultPortfolio()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var defaultPortfolio = new Portfolio()
            {
                Id = Guid.NewGuid(),
                Name = "Default Portfolio",
                UserId = userId,
                IsDefault = true,
                Currency = "AUD"
            };

            _mockPortfolioRepository
                .Setup(r => r.GetDefaultPortfolioAsync(userId))
                .ReturnsAsync(defaultPortfolio);

            // Act
            var result = await _portfolioService.GetDefaultPortfolioAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Default Portfolio");
            result.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task GetDefaultPortfolioAsync_WhenNoDefault_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockPortfolioRepository
                .Setup(r => r.GetDefaultPortfolioAsync(userId))
                .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _portfolioService.GetDefaultPortfolioAsync(userId);

            // Assert
            result.Should().BeNull();
        }
    }
}
