using Microsoft.AspNetCore.Mvc;
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
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    // Adding constraint (guid) to the route parameter
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        _logger.LogInformation("GET /api/users/{UserId}", id);

        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="createUserDto">User registration details</param>
    /// <returns>Created user</returns>
    [HttpPost]
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
    /// <param name="id">User ID</param>
    /// <param name="updateUserDto">Updated user details</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        _logger.LogInformation("PUT /api/users/{UserId}", id);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
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
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        _logger.LogInformation("DELETE /api/users/{UserId}", id);

        var deleted = await _userService.DeleteUserAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }

        return NoContent();
    }
}