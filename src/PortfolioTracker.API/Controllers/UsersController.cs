using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.API.Extensions;
using PortfolioTracker.Core.DTOs.User;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.API.Controllers;

/// <summary>
/// Controller for managing user accounts.
/// This controller is now THIN - it only handles HTTP concerns.
/// All business logic is in UserService.
/// </summary>
/// <remarks>
/// Controller Responsibilities (Keep it THIN!):
/// 1. HTTP request/response handling
/// 2. Route mapping
/// 3. Status code decisions
/// 4. Input validation (basic)
/// 5. Calling services
/// 
/// What Controllers Should NOT Do:
/// DON'T DO : Business logic (that's in services)
/// DON'T DO : Database access (that's in repositories/services)
/// DON'T DO : Complex validation (that's in services/validators)
/// DON'T DO : Data mapping (that's in services)
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Constructor with Dependency Injection
    /// </summary>
    /// <param name="userService">User Service instance</param>
    /// <param name="logger">Logger for this controller</param>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (for testing purposes - not for production)
    /// </summary>
    /// <returns>List of all users</returns>
    /// todo: revisit the need for this endpoint before deploying this app to production 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        _logger.LogInformation("GET /api/users - Retrieving all users");

        var users = await _userService.GetAllUsersAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User details</returns>
    // Adding constraint (guid) to the route parameter
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // todo: add 403 test cases for the auth checks in this and other methods
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid userId)
    {
        _logger.LogInformation("GET /api/users/{UserId}", userId);

        // Authorization: User can only access their own data
        if (!User.IsAuthorizedForUser(userId))
        {
            _logger.LogWarning("User {GetAuthenticatedUserId} attempted to access user {UserId}", User.GetAuthenticatedUserId(), userId);

            return Forbid();
        }

        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = $"User with ID {userId} not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="createUserDto">User registration details</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        _logger.LogInformation("POST /api/users - Creating user: {Email}", createUserDto.Email);

        // Model validation happens automatically via [Required], [EmailAddress] attributes
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.CreateUserAsync(createUserDto);

            // Return 201 Created with location header
            return CreatedAtAction(
                nameof(GetUser),
                new { id = user!.Id },
                user
            );
        }
        catch (InvalidOperationException ex)
        {
            // Business logic exception (e.g., email already exists)
            _logger.LogWarning("Failed to create user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update user details.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="updateUserDto">Updated user details</param>
    /// <returns>Updated user</returns>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        _logger.LogInformation("PUT /api/users/{UserId}", userId);

        if (!User.IsAuthorizedForUser(userId))
        {
            _logger.LogWarning("User {GetAuthenticatedUserId} attempted to update user {UserId}", User.GetAuthenticatedUserId(), userId);

            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.UpdateUserAsync(userId, updateUserDto);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {userId} not found" });
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        _logger.LogInformation("DELETE /api/users/{UserId}", userId);

        if (!User.IsAuthorizedForUser(userId))
        {
            _logger.LogWarning("User {GetAuthenticatedUserId} attempted to delete user {UserId}", User.GetAuthenticatedUserId(), userId);

            return Forbid();
        }

        var deleted = await _userService.DeleteUserAsync(userId);

        if (!deleted)
        {
            return NotFound(new { message = $"User with ID {userId} not found" });
        }

        return NoContent();
    }
}