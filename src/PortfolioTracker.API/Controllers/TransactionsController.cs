using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.API.Extensions;
using PortfolioTracker.Core.DTOs.Transaction;
using PortfolioTracker.Core.Interfaces.Repositories;

namespace PortfolioTracker.API.Controllers;

/// <summary>
/// Manages buy and sell transactions for holdings.
/// Automatically updates holding positions and average cost.
/// </summary>

[Authorize]
[ApiController]
[Route("api/users/{userId}")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    // NOTE: The API can expose transactions at both the portfolio and holding level, depending on use case.

    /// <summary>
    /// Get all transactions for a specific holding.
    /// GET /api/users/{userId}/holdings/{holdingId}/transactions
    /// </summary>
    [HttpGet("holdings/{holdingId}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetHoldingTransactions(
        Guid userId,
        Guid holdingId)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            return Forbid();
        }

        var transactions = await _transactionService.GetHoldingTransactionsAsync(holdingId, userId);

        return Ok(transactions);
    }

    /// <summary>
    /// Get all transactions for a portfolio.
    /// GET /api/users/{userId}/portfolios/{portfolioId}/transactions
    /// </summary>
    [HttpGet("portfolios/{portfolioId}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetPortfolioTransactions(
        Guid userId,
        Guid portfolioId)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            return Forbid();
        }

        var transactions = await _transactionService.GetPortfolioTransactionsAsync(portfolioId, userId);

        return Ok(transactions);
    }

    /// <summary>
    /// Get a specific transaction by ID.
    /// </summary>
    [HttpGet("transactions/{transactionId}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> GetTransaction(
        Guid userId,
        Guid transactionId)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            return Forbid();
        }

        var transaction = await _transactionService.GetTransactionByIdAsync(transactionId, userId);

        if (transaction == null)
        {
            return NotFound(new {message = "Transaction not found"});
        }

        return Ok(transaction);
    }

    [HttpPost("transactions")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(
        Guid userId,
        [FromBody] CreateTransactionDto createTransactionDto)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            return Forbid();
        }

        try
        {
            var transaction = await _transactionService.CreateTransactionAsync(userId, createTransactionDto);

            return CreatedAtAction(
                nameof(GetTransaction),
                new {userId, transactionId = transaction.Id},
                transaction);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create transaction for user {UserId}", userId);
            return BadRequest(new {message = ex.Message});
        }
    }

    /// <summary>
    /// Update an existing transaction.
    /// PUT /api/users/{userId}/transactions/{transactionId}
    /// Recalculates holding position.
    /// </summary>
    [HttpPut("transactions/{transactionId}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(
        Guid userId,
        Guid transactionId,
        [FromBody] UpdateTransactionDto updateTransactionDto)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            return Forbid();
        }

        try
        {
            var transaction =
                await _transactionService.UpdateTransactionAsync(transactionId, userId, updateTransactionDto);

            if (transaction == null)
            {
                return NotFound(new {message = "Transaction not found"});
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update transaction {TransactionId} for user {UserId}", transactionId,
                userId);
            return BadRequest(new {message = ex.Message});
        }
    }

    /// <summary>
    /// Delete a transaction.
    /// DELETE /api/users/{userId}/transactions/{transactionId}
    /// Recalculates holding position.
    /// </summary>
    [HttpDelete("transactions/{transactionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransaction(
        Guid userId,
        Guid transactionId)
    {
        if (!User.IsAuthorizedForUser(userId))
        {
            return Forbid();
        }

        var deleted = await _transactionService.DeleteTransactionAsync(transactionId, userId);
        if (!deleted)
        {
            return NotFound(new { message = "Transaction not found" });
        }

        return NoContent();
    }
}
