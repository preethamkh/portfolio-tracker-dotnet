using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.DTOs.User;
using PortfolioTracker.IntegrationTests.Fixtures;
using PortfolioTracker.IntegrationTests.Helpers;

namespace PortfolioTracker.IntegrationTests.API;

public class UsersControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region GET /api/users - Get

    [Fact]
    public async Task GetUsers_WhenUsersExist_ReturnsUserList()
    {
        // Arrange
        // Create test users directly in the database
        await TestDataBuilder.CreateUser(Context, email: "user1@test.com");
        await TestDataBuilder.CreateUser(Context, email: "user2@test.com");
        await TestDataBuilder.CreateUser(Context, email: "user3@test.com");

        // Authenticate as any user to access the endpoint
        await RegisterAndAuthenticateAsync();

        // Act
        // Point to Note:
        // The test code (direct DB access) and the API (via HTTP calls) each resolve their own DbContext from their own DI container, and if the database names differ, they do not share data.
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.ReadAsJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().HaveCount(4); // 3 created + 1 authenticated

        // Verify returned users match created users
        users.Should().Contain(u => u.Email == "user1@test.com");
        users.Should().Contain(u => u.Email == "user2@test.com");
        users.Should().Contain(u => u.Email == "user3@test.com");
    }

    [Fact]
    public async Task GetUser_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync(email: "preetham@test.com", fullName: "Preetham K H");

        // Act
        var response = await Client.GetAsync($"/api/users/{authResponse.User.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedUser = await response.ReadAsJsonAsync<UserDto>();
        returnedUser.Should().NotBeNull();
        returnedUser.Id.Should().Be(authResponse.User.Id);
        returnedUser.Email.Should().Be("preetham@test.com");
        returnedUser.FullName.Should().Be("Preetham K H");
    }

    [Fact]
    public async Task GetUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange: Register and authenticate a user
        var authResponse = await RegisterAndAuthenticateAsync();
        var userId = authResponse.User.Id;

        // Delete the user directly from the DB
        var user = await Context.Users.FindAsync(userId);
        Context.Users.Remove(user!);
        await Context.SaveChangesAsync();

        // Act: Try to get the deleted user (your own userId, but no longer exists)
        var response = await Client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/users - Create 

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreatedUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "preetham@test.com",
            FullName = "Preetham K H",
            Password = "SecurePassword123!"        
        };

        // Act
        var response = await Client.PostAsJsonAsync("api/users", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdUser = await response.ReadAsJsonAsync<UserDto>();
        createdUser.Should().NotBeNull();
        createdUser.Email.Should().Be("preetham@test.com");
        createdUser.FullName.Should().Be("Preetham K H");
        createdUser.Id.Should().NotBeEmpty();

        // Verify user was actually created in the database
        var userInDatabase = await Context.Users.FirstOrDefaultAsync(u => u.Id == createdUser.Id);
        userInDatabase.Should().NotBeNull();
        userInDatabase.Email.Should().Be("preetham@test.com");
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        // existing user with same email
        await TestDataBuilder.CreateUser(Context, email: "preetham@test.com");

        var createUserDto = new CreateUserDto
        {
            Email = "preetham@test.com",
            FullName = "Preetham K H",
            Password = "SecurePassword123!"
        };

        // Act
        var response =  await Client.PostAsJsonAsync("api/users", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "invalid-email-format",
            Password = "short",
            FullName = new string('A', 300) // Exceeds max length
        };

        // Act
        var response = await Client.PostAsJsonAsync("api/users", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithMissingFields_ReturnsBadRequest()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "preetham@test.com",
            Password = "",
            FullName = "Preetham K H"
        };

        // Act
        var response = await Client.PostAsJsonAsync("api/users", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/users/{userId} - Update 
    
    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsUpdatedUser()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync();
        var userId = authResponse.User.Id;

        var updateUserDto = new UpdateUserDto
        {
            Email = "updatedemail@test.com",
            FullName = "Updated Name"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"api/users/{userId}", updateUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await response.ReadAsJsonAsync<UserDto>();
        updatedUser.Should().NotBeNull();
        updatedUser.Email.Should().Be("updatedemail@test.com");
        updatedUser.FullName.Should().Be("Updated Name");
        updatedUser.Id.Should().Be(userId);

        // Force EF Core to reload the entity from the DB bypassing the cache.
        // This is because when we update, if we use FindAsync() or similar on a context that already
        // tracked the enitity, it may return the cached (pre-update) version, not the lates from the DB.
        var userInDatabase = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        userInDatabase.Should().NotBeNull();
        userInDatabase.Email.Should().Be("updatedemail@test.com");
        userInDatabase.FullName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        //var nonExistentUserId = Guid.NewGuid();

        // Arrange: Register and authenticate a user
        var authResponse = await RegisterAndAuthenticateAsync();
        var userId = authResponse.User.Id;

        // Delete the user directly from the DB
        var user = await Context.Users.FindAsync(userId);
        Context.Users.Remove(user!);
        await Context.SaveChangesAsync();

        var updateUserDto = new UpdateUserDto
        {
            Email = "updatedemail@test.com",
            FullName = "Updated Name"
        };

        // Act - on the non-existent user
        var response = await Client.PutAsJsonAsync($"api/users/{userId}", updateUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/users/{userId} - Delete

    [Fact]
    public async Task DeleteUser_WhenUserExists_ReturnsNoContent()
    {
        // Arrange
        var authResponse = await RegisterAndAuthenticateAsync();
        var userId = authResponse.User.Id;

        // Act
        var response = await Client.DeleteAsync($"api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user was deleted from the DB
        var userInDatabase = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        userInDatabase.Should().BeNull();
    }


    [Fact(Skip = "Temporarily disabled due to known in-memory cascade deletion issue and wanting to swap to real DB for the test")]
    public async Task DeleteUser_WithPortfolios_DeletesCascade()
    {
        // ARRANGE
        var authResponse = await RegisterAndAuthenticateAsync();
        var userId = authResponse.User.Id;

        var portfolio = await TestDataBuilder.CreatePortfolio(Context, userId);

        // ACT - Delete user
        var response = await Client.DeleteAsync($"/api/users/{userId}");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // todo: comment this for now as I want to use a real database backed integration test
        // since there is an issue with in-memory db and cascade delete
        // Verify user deleted - FindAsync() checks cache first, so use ReloadFromDb to force fresh query
        // var userInDb = await Context.Users.FindAsync(user.Id);

        var userInDatabase = await ReloadFromDb(authResponse.User);
        userInDatabase.Should().BeNull();

        // Verify portfolio also deleted (cascade!)
        var portfolioInDatabase = await ReloadFromDb(portfolio);
        portfolioInDatabase.Should().BeNull();
    }


    #endregion

}