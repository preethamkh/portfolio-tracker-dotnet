using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Core.DTOs.Auth;
using PortfolioTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioTracker.API.Controllers;

/// <summary>
/// Controller for authentication operations (register, login).
/// </summary>
/// <remarks>
/// This controller handles user authentication without requiring existing authentication.
/// It's the entry point for users to access the system.
/// 
/// Endpoints:
/// - POST /api/auth/register - Register new user
/// - POST /api/auth/login - Login existing user
/// 
/// Security Notes:
/// - These endpoints are [AllowAnonymous] (no auth required)
/// - All other endpoints will require [Authorize] attribute
/// - Passwords are never returned in responses
/// - Failed login attempts should use generic error messages
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            var response = await authService.RegisterAsync(request);

            logger.LogInformation("User registered successfully: {Email}", request.Email);

            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            // Business rule violation (e.g., email already exists)
            logger.LogWarning(exception, "Registration failed for email: {Email}", request.Email);
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            // Unexpected error
            logger.LogError(exception, "Unexpected error during registration for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Authenticates a user and returns JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var response = await authService.LoginAsync(request);

            logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(response);
        }
        catch (UnauthorizedAccessException exception)
        {
            // Invalid credentials
            logger.LogWarning(exception, "Login failed for email: {Email}", request.Email);

            return Unauthorized(new { message = "Invalid email or password" });
        }
        catch (Exception exception)
        {
            // Unexpected error
            logger.LogError(exception, "Unexpected error during login for email: {Email}", request.Email);

            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>Current user information</returns>
    /// <response code="200">Returns current user info</response>
    /// <response code="401">Not authenticated</response>
    /// <remarks>
    /// This endpoint demonstrates how to use [Authorize] attribute.
    /// </remarks>
    [HttpGet("me")]
    [Authorize] // Requires valid JWT token
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        // User.Identity is populated by JWT middleware
        // It contains claims from the token

        // Extract user ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        // Extract email from claims
        var email = User.Identity?.Name ?? string.Empty;

        // Extract full name from claims
        var fullName = User.FindFirst("name")?.Value;

        return Ok(new UserInfo
        {
            Id = userId,
            Email = email,
            FullName = fullName
        });
    }
}
