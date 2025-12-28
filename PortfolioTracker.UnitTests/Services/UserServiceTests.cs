using FluentAssertions;
using Moq;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Services;
using Xunit.Sdk;

namespace PortfolioTracker.UnitTests.Services;

/// <summary>
/// Unit tests for UserService.
/// These tests verify business logic in isolation by mocking dependencies.
/// </summary>
/// <remarks>
/// Test Organization:
/// - Tests are grouped by method using #region directives.
/// - Each method has multiple tests cases (happy path, edge cases, error handling).
/// - Test names follow pattern: MethodName_Scenario_ExpectedBehavior.
///
/// Why this pattern?
/// - Clear what's being tested
/// - Clear what scenario we're testing
/// - Clear what we expect to happen
/// - Example: CreateUserAsync_WithDuplicateEmail_ShouldThrowsException 
/// </remarks>
public class UserServiceTests : TestBase
{
    // Dependencies as mocks
    private readonly Mock<IUserRepository> _mockUserRepository;

    // System under test
    private readonly UserService _userService;

    /// <summary>
    /// Constructor runs BEFORE EACH test.
    /// This ensures each test starts with fresh mocks and state. (test isolation)
    /// </summary>
    /// <remarks>
    /// Why in constructor?
    /// - xUnit creates a new instance of the test class for each test method.
    /// - This gives us clean mocks for every test without extra setup/teardown code. (no state between shared tests)
    /// - Alternative: [SetUp] method (NUnit) or [TestInitialize] (MSTest) would require more boilerplate, but xUnit uses constructor approach.
    /// </remarks>
    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();

        _userService = new UserService(_mockUserRepository.Object, CreateMockLogger<UserService>());
    }

    /// <summary>
    /// Test: GetAllUsersAsync - Happy Path - when users exist in database.
    /// Scenario: Repository returns multiple users.
    /// Expected: Service returns all users as DTOs.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetAllUsersAsync_WhenUsersExist_ShouldReturnAllUsers()
    {
        // Arrange
        // Create mock data with fake users (like they would be in the database)
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user1@test.com",
                FullName = "User One",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user2@test.com",
                FullName = "User Two",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Configure mock repository to return the fake users
        // When GetAllAsync is called, return our mock users
        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        // Call the method under test - GetAllUsersAsync - this is what we're testing
        // ToList() forces the query to execute once and stores the results in memory.
        // All further enumerations just read from the in-memory list, not the original source.
        var result = (await _userService.GetAllUsersAsync()).ToList();

        // Assert
        // Verify the results are correct
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        // Verify first user details
        var firstUser = result.First();
        firstUser.Email.Should().Be("user1@test.com");
        firstUser.FullName.Should().Be("User One");

        // Verify that the repository method was called exactly once
        _mockUserRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// Test: GetAllUsersAsync when no users exist.
    /// Scenario: Repository returns empty list.
    /// Expected: Service returns empty list (not null, not error).
    /// </summary>
    /// <remarks>
    /// Why test this?
    /// - Empty state is common (new system, all users deleted, etc.)
    /// - Service should handle gracefully (not crash!)
    /// - Should return empty collection, not null (defensive programming)
    /// </remarks>
    [Fact]
    public async Task GetAllUsersAsync_WhenNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = (await _userService.GetAllUsersAsync()).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User()
        {
            Id = userId,
            Email = "kh@gmail.com",
            FullName = "Preetham K H",
            CreatedAt = DateTime.UtcNow,
            LastLogin = null
        };

        // Mock setup: when looking for this user ID, return the user
        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("kh@gmail.com");
        result.FullName.Should().Be("Preetham K H");

        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }
}