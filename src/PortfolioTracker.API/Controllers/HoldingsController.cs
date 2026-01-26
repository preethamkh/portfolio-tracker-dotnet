using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.API.Extensions;
using PortfolioTracker.Core.DTOs.Holding;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.API.Controllers;

/// <summary>
/// Manages holdings (securities) within user portfolios.
/// All endpoints require authentication and verify user ownership of the portfolio.
/// </summary>
[Authorize]
[ApiController]
[Route("api/users/{userId}/portfolios/{portfolioId}/holdings")]
public class HoldingsController(IHoldingService holdingService, ILogger<HoldingsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all holdings in a portfolio with current prices and valuations.
    /// GET /api/users/{userId}/portfolios/{portfolioId}/holdings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HoldingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<HoldingDto>>> GetPortfolioHoldings(Guid portfolioId, Guid userId)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            logger.LogWarning("User {AuthUserId} attempted to access holdings for user {UserId}",
                User.GetAuthenticatedUserId(), userId);
            return Forbid();
        }

        var holdings = await holdingService.GetPortfolioHoldingsAsync(portfolioId, userId);

        // The 404 status should be reserved for when the portfolio itself doesn't exist, not when it has zero holdings. This is the correct RESTful API design.
        return Ok(holdings);
    }

    /// <summary>
    /// Get a specific holding by ID with current valuation.
    /// GET /api/users/{userId}/portfolios/{portfolioId}/holdings/{holdingId}
    /// </summary>
    [HttpGet("{holdingId:guid}")]
    [ProducesResponseType(typeof(HoldingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HoldingDto>> GetHolding(Guid holdingId, Guid portfolioId, Guid userId)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            logger.LogWarning("User {AuthUserId} attempted to access holdings for user {UserId}",
                User.GetAuthenticatedUserId(), userId);
            return Forbid();
        }

        var holding = await holdingService.GetHoldingByIdAsync(holdingId, portfolioId, userId);

        if (holding == null)
        {
            return NotFound(new { message = "Holding not found" });
        }

        return Ok(holding);
    }

    /// <summary>
    /// Add a new holding to a portfolio.
    /// POST /api/users/{userId}/portfolios/{portfolioId}/holdings
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HoldingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HoldingDto>> CreateHolding(Guid portfolioId, Guid userId,  [FromBody]CreateHoldingDto createHoldingDto)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            logger.LogWarning("User {AuthUserId} attempted to access holdings for user {UserId}",
                User.GetAuthenticatedUserId(), userId);
            return Forbid();
        }

        try
        {
            var holding = await holdingService.CreateHoldingAsync(portfolioId, userId, createHoldingDto);

            if (holding == null)
            {
                return NotFound(new { message = "Portfolio or user not found" });
            }

            // params are as follows:
            // 1. name of the action to retrieve the created resource
            // 2. route values (including IDs) for the created resource
            // 3. the created resource itself to return in the response body
            return CreatedAtAction(nameof(GetHolding), new { userId, portfolioId, holdingId = holding.HoldingId }, holding);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create holding in portfolio {PortfolioId}", portfolioId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update holding shares and average cost.
    /// PUT /api/users/{userId}/portfolios/{portfolioId}/holdings/{holdingId}
    /// </summary>
    [HttpPut("{holdingId}")]
    [ProducesResponseType(typeof(HoldingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HoldingDto>> UpdateHolding(
        Guid userId,
        Guid portfolioId,
        Guid holdingId,
        [FromBody] UpdateHoldingDto updateHoldingDto)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            return Forbid();
        }

        var holding = await holdingService.UpdateHoldingAsync(holdingId, portfolioId, userId, updateHoldingDto);
        if (holding == null)
        {
            return NotFound(new { message = "Holding not found" });
        }

        return Ok(holding);
    }

    /// <summary>
    /// Delete a holding from a portfolio.
    /// DELETE /api/users/{userId}/portfolios/{portfolioId}/holdings/{holdingId}
    /// </summary>
    [HttpDelete("{holdingId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHolding(Guid holdingId, Guid portfolioId, Guid userId)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            logger.LogWarning("User {AuthUserId} attempted to access holdings for user {UserId}",
                User.GetAuthenticatedUserId(), userId);
            return Forbid();
        }
        var success = await holdingService.DeleteHoldingAsync(holdingId, portfolioId, userId);
        if (!success)
        {
            return NotFound(new { message = "Holding not found" });
        }
        return NoContent();
    }
}
