using FluentAssertions;
using Moq;
using PortfolioTracker.Core.DTOs.User;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Services;

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

    /// <summary>
    /// Test: Get user by ID when user exists - happy path.
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User()
        {
            Id = userId,
            Email = "user1@test.com",
            FullName = "User One",
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
        result.Email.Should().Be("user1@test.com");
        result.FullName.Should().Be("User One");

        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }

    /// <summary>
    /// Test: Get user by ID when user does not exist.
    /// To test error handling.
    /// </summary>
    /// <remarks>
    /// Why return null instead of throwing exception?
    /// - "Not found" is not an error - it's a valid business scenario
    /// - Exceptions should be for exceptional conditions (database down, etc.)
    /// - Null allows caller to handle gracefully (return 404 to client)
    /// 
    /// Alternative approaches:
    /// - Return Result<UserDto/> with success/failure
    /// - Throw NotFoundException
    /// - This approach: Return null, let controller convert to 404
    /// </remarks>
    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?) null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();

        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }

    /// <summary>
    /// Test: Get user by email when user exists - happy path.
    /// </summary>
    [Fact]
    public async Task GetUserByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        const string email = "user1@test.com";

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = "User One",
            CreatedAt = DateTime.UtcNow,
            LastLogin = null
        };

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("user1@test.com");
        result.FullName.Should().Be("User One");

        _mockUserRepository.Verify(repo => repo.GetByEmailAsync(email), Times.Once);
    }

    /// <summary>
    /// Test: Get user by email when user does not exist - error path. (returns null)
    /// </summary>
    [Fact]
    public async Task GetUserByEmailAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        const string email = "user1@test.com";

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync((User?) null);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();

        _mockUserRepository.Verify(repo => repo.GetByEmailAsync(email), Times.Once);
    }

    /// <summary>
    /// Create user with valid data - happy path.
    /// An important test - verifies user creation logic / core functionality.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "user1@test.com",
            Password = "Password123!",
            FullName = "User One"
        };

        // to ensure the mock 1, 2, 3 are called in order the CreateUserAsync() method of the UserService
        // if any of the methods get called in a different order, this will highlight / fail
        var mockCallSequence = new MockSequence();

        // Mock one: Email is not taken
        // Business rule: check email availability before creating
        _mockUserRepository.InSequence(sequence: mockCallSequence)
            .Setup(repo => repo.IsEmailTakenAsync(createUserDto.Email, null))
            .ReturnsAsync(false);

        // Mock two: AddAsync - simulate adding user to database, should succeed
        _mockUserRepository.InSequence(sequence: mockCallSequence)
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                // simulate what the database would do - generate a new ID
                if (user.Id == Guid.Empty)
                {
                    user.Id = Guid.NewGuid();
                }
            })
            .ReturnsAsync((User u) => u);

        // or whatever properties (can't figure out the ID as it's generated), hence use It.IsAny<Guid>()
        //_mockUserRepository.Verify(repo => repo.AddAsync(
        //    It.Is<User>(u =>
        //            u.Email == createUserDto.Email &&
        //            u.FullName == createUserDto.FullName &&
        //            !string.IsNullOrEmpty(u.PasswordHash)
        //    )), Times.Once);

        // Mock three: SaveChangesAsync - simulate saving to database, should succeed
        _mockUserRepository.InSequence(sequence: mockCallSequence)
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1); // 1 row affected - EF core convention

        // Act
        var result = await _userService.CreateUserAsync(createUserDto);

        // Assert
        // verify DTO was created correctly
        result.Should().NotBeNull();
        result.Email.Should().Be(createUserDto.Email);
        result.FullName.Should().Be(createUserDto.FullName);
        // swagger would create the user ID correctly (ORM ensures this)
        // but during test, since I am supplying the values, I need to ensure the CreateUserAsync()
        // has the Id = Guid.NewGuid() set so that the guid is non-empty, since I am mocking the repository and controlling the flow.
        // see : Mock two: AddAsync above
        result.Id.Should().NotBe(Guid.Empty);

        // verify all repository methods were called in the correct order
        _mockUserRepository.Verify(repo => repo.IsEmailTakenAsync(createUserDto.Email, null), Times.Once());

        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once());

        _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Test: Create user with duplicate email (business rule violation).
    /// This tests business logic enforcement.
    /// Short circuits when rule is violated.
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "user1@test.com",
            Password = "Password123!",
            FullName = "User One"
        };

        // Mock: Email is already taken
        _mockUserRepository
            .Setup(repo => repo.IsEmailTakenAsync(createUserDto.Email, null))
            .ReturnsAsync(true);

        // Act
        // For asserting exceptions from async methods, we need to wrap the call in a Func<Task>
        Func<Task> createUserAction = async () => await _userService.CreateUserAsync(createUserDto);

        // Assert
        await createUserAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with email {createUserDto.Email} already exists");

        _mockUserRepository.Verify(repo => repo.IsEmailTakenAsync(createUserDto.Email, null), Times.Once);

        // Ensure AddAsync and SaveChangesAsync were never called
        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    /// Test: Update user with valid data.
    /// Tests the happy path for updates.
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Email = "user1@test.com",
            FullName = "User One",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var updateUserDto = new UpdateUserDto
        {
            Email = "user1.updated@test.com",
            FullName = "Updated User One"
        };

        // Mock: GetByIdAsync returns existing user
        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Mock: Email is not taken
        _mockUserRepository
            .Setup(repo => repo.IsEmailTakenAsync(updateUserDto.Email, userId))
            .ReturnsAsync(false);

        // Mock: UpdateAsync
        _mockUserRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Mock: SaveChangesAsync
        _mockUserRepository
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateUserDto);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(updateUserDto.Email);
        result.FullName.Should().Be(updateUserDto.FullName);

        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Test: Update non-existent user.
    /// Tests authorization/not-found scenario.
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { Email = "new@test.com" };

        // Mock: User not found
        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?) null);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Should().BeNull();

        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Test: Update user with email that's taken by another user.
    /// Tests business rule enforcement during updates.
    /// </summary>
    /// <remarks>
    /// Scenario: User A tries to change email to email already owned by User B
    /// Expected: Reject the update
    /// 
    /// Key Difference from Create:
    /// - Create: Check if email exists AT ALL
    /// - Update: Check if email exists for ANOTHER user (not this user)
    /// </remarks>
    [Fact]
    public async Task UpdateUserAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Email = "user1@test.com" };
        var updateDto = new UpdateUserDto { Email = "taken@test.com" };

        // Mock: GetByIdAsync returns existing user
        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Mock: Email is taken by another user
        _mockUserRepository
            .Setup(repo => repo.IsEmailTakenAsync(updateDto.Email, userId))
            .ReturnsAsync(true);

        // Act
        Func<Task> updateUserAction = async () => await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        await updateUserAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Email {updateDto.Email} is already taken");

        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Delete existing user - happy path.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Email = "user1@test.com" };

        // Mock: GetByIdAsync returns existing user
        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Mock: DeleteAsync
        _mockUserRepository
            .Setup(repo => repo.DeleteAsync(existingUser))
            .Returns(Task.CompletedTask);

        // Mock: SaveChangesAsync
        _mockUserRepository
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();

        _mockUserRepository.Verify(repo => repo.DeleteAsync(existingUser), Times.Once);
        _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Delete non-existent user.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteUserAsync_WhenUserNotFound_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Mock: User not found
        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();

        _mockUserRepository.Verify(repo => repo.DeleteAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}