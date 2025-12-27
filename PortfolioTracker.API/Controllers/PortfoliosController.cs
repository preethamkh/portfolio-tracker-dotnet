using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Core.DTOs.Portfolio;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.API.Controllers;

/// <summary>
/// Controller for managing investment portfolios.
/// Demonstrates proper authorization pattern - userId comes from route for now.
/// In production, userId would come from authenticated JWT token.
/// </summary>
/// <remarks>
/// Authorization Pattern:
/// - All methods take userId parameter (simulating authentication)
/// - Service layer enforces user can only access their own portfolios
/// - Later we'll replace userId param with JWT claims
/// </remarks>
[ApiController]
[Route("api/users/{userId}/portfolios")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly ILogger<PortfoliosController> _logger;

    public PortfoliosController(
        IPortfolioService portfolioService,
        ILogger<PortfoliosController> logger)
    {
        _portfolioService = portfolioService;
        _logger = logger;
    }

    /// <summary>
    /// Get all portfolios for a user.
    /// </summary>
    /// <param name="userId">User ID from route</param>
    /// <returns>List of user's portfolios</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PortfolioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetUserPortfolios(Guid userId)
    {
        _logger.LogInformation("GET /api/users/{UserId}/portfolios", userId);

        var portfolios = await _portfolioService.GetUserPortfoliosAsync(userId);

        return Ok(portfolios);
    }

    /// <summary>
    /// Get a specific portfolio by ID.
    /// </summary>
    /// <param name="userId">User ID from route</param>
    /// <param name="id">Portfolio ID</param>
    /// <returns>Portfolio details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioDto>> GetPortfolio(Guid userId, Guid id)
    {
        _logger.LogInformation("GET /api/users/{UserId}/portfolios/{PortfolioId}", userId, id);

        var portfolio = await _portfolioService.GetPortfolioByIdAsync(id, userId);

        if (portfolio == null)
        {
            return NotFound(new { message = "Portfolio not found or you don't have access" });
        }

        return Ok(portfolio);
    }

    /// <summary>
    /// Get the user's default portfolio.
    /// </summary>
    /// <param name="userId">User ID from route</param>
    /// <returns>Default portfolio or 404</returns>
    [HttpGet("default")]
    [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioDto>> GetDefaultPortfolio(Guid userId)
    {
        _logger.LogInformation("GET /api/users/{UserId}/portfolios/default", userId);

        var portfolio = await _portfolioService.GetDefaultPortfolioAsync(userId);

        if (portfolio == null)
        {
            return NotFound(new { message = "No default portfolio set" });
        }

        return Ok(portfolio);
    }

    /// <summary>
    /// Create a new portfolio.
    /// </summary>
    /// <param name="userId">User ID from route</param>
    /// <param name="createPortfolioDto">Portfolio creation data</param>
    /// <returns>Created portfolio</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PortfolioDto>> CreatePortfolio(
        Guid userId,
        [FromBody] CreatePortfolioDto createPortfolioDto)
    {
        _logger.LogInformation("POST /api/users/{UserId}/portfolios - Creating: {Name}",
            userId, createPortfolioDto.Name);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var portfolio = await _portfolioService.CreatePortfolioAsync(userId, createPortfolioDto);

            return CreatedAtAction(
                nameof(GetPortfolio),
                new { userId, id = portfolio.Id },
                portfolio
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create portfolio: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update portfolio details.
    /// </summary>
    /// <param name="userId">User ID from route</param>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <param name="updatePortfolioDto">Updated portfolio data</param>
    /// <returns>Updated portfolio</returns>
    [HttpPut("{portfolioId:guid}")]
    [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PortfolioDto>> UpdatePortfolio(
        Guid userId,
        Guid portfolioId,
        [FromBody] UpdatePortfolioDto updatePortfolioDto)
    {
        _logger.LogInformation("PUT /api/users/{UserId}/portfolios/{PortfolioId}", userId, portfolioId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var portfolio = await _portfolioService.UpdatePortfolioAsync(portfolioId, userId, updatePortfolioDto);

            if (portfolio == null)
            {
                return NotFound(new { message = "Portfolio not found or you don't have access" });
            }

            return Ok(portfolio);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update portfolio: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a portfolio.
    /// </summary>
    /// <param name="userId">User ID from route</param>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{portfolioId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePortfolio(Guid userId, Guid portfolioId)
    {
        _logger.LogInformation("DELETE /api/users/{UserId}/portfolios/{PortfolioId}", userId, portfolioId);

        var deleted = await _portfolioService.DeletePortfolioAsync(portfolioId, userId);

        if (!deleted)
        {
            return NotFound(new { message = "Portfolio not found or you don't have access" });
        }

        return NoContent();
    }

    /// <summary>
    /// Set a portfolio as the user's default.
    /// </summary>
    /// <param name="userId">User ID from route</param>
    /// <param name="id">Portfolio ID</param>
    /// <returns>No content</returns>
    [HttpPost("{id}/set-default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAsDefault(Guid userId, Guid id)
    {
        _logger.LogInformation("POST /api/users/{UserId}/portfolios/{PortfolioId}/set-default",
            userId, id);

        var success = await _portfolioService.SetAsDefaultAsync(id, userId);

        if (!success)
        {
            return NotFound(new { message = "Portfolio not found or you don't have access" });
        }

        return NoContent();
    }
}