using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Core.DTOs.Security;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.API.Controllers;

/// <summary>
/// Manages Securities (Stocks, ETFs, etc.) in the system.
/// Provides search and lookup functionality.
/// </summary>
[Authorize]
[ApiController]
[Route("api/securities")]
public class SecuritiesController(ISecurityService securityService, ILogger<SecuritiesController> logger) : ControllerBase
{
    /// <summary>
    /// Search for securities by symbol or name
    /// Search external API and local database
    /// GET /api/securities/search?query=apple&;limit=10
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<SecurityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SecurityDto>>> SearchSecurities([FromQuery] string query,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new {message = "Query parameter is required" });
        }

        if (limit is < 1 or > 10)
        {
            return BadRequest(new { message = "Limit must be between 1 and 10" });
        }

        var securities = await securityService.SearchSecuritiesAsync(query, limit);

        return Ok(securities);
    }

    /// <summary>
    /// Get security by ID from local database
    /// GET /api/securities/{securityId}
    /// </summary>
    [HttpGet("{securityId:guid}")]
    [ProducesResponseType(typeof(SecurityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SecurityDto>> GetSecurityById(Guid securityId)
    {
        var security = await securityService.GetSecurityByIdAsync(securityId);

        if (security == null)
        {
            return NotFound(new {message = $"Security with {securityId} not found"});
        }

        return Ok(security);
    }

    /// <summary>
    /// Get security by symbol from local database.
    /// GET /api/securities/symbol/AAPL
    /// </summary>
    [HttpGet("symbol/{symbol}")]
    [ProducesResponseType(typeof(SecurityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SecurityDto>> GetSecurityBySymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest(new { message = "Symbol is required" });
        }

        var security = await securityService.GetSecurityBySymbolAsync(symbol);

        if (security == null)
        {
            return NotFound(new { message = $"Security with symbol {symbol} not found" });
        }

        return Ok(security);
    }

    /// <summary>
    /// Get or create a security by symbol.
    /// If exists in DB, returns it. If not, fetches from external API and creates it.
    /// Useful when adding holdings - ensures security exists.
    /// POST /api/securities/get-or-create
    /// Body: { "symbol": "AAPL" }
    /// </summary>
    [HttpPost("get-or-create")]
    [ProducesResponseType(typeof(SecurityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SecurityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SecurityDto>> GetOrCreateSecurity([FromBody] GetOrCreateSecurityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            return BadRequest(new { message = "Symbol is required" });
        }

        try
        {
            // Check if it already exists
            var existingSecurity = await securityService.GetSecurityBySymbolAsync(request.Symbol);
            if (existingSecurity != null)
            {
                return Ok(existingSecurity);
            }

            // Create new security
            var security = await securityService.GetOrCreateSecurityAsync(request.Symbol);
            return CreatedAtAction(
                nameof(GetSecurityById),
                new { securityId = security.Id },
                security);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to get or create security for symbol {Symbol}", request.Symbol);
            return BadRequest(new { message = ex.Message });
        }
    }

    // todo: FYI - not sure if I want to keep this here or move to a separate location.
    /// <summary>
    /// Request model for get-or-create endpoint.
    /// </summary>
    public class GetOrCreateSecurityRequest
    {
        public string Symbol { get; set; } = string.Empty;
    }
}
