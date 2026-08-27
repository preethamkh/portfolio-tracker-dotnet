namespace BlazorFrontend.Models;

/// <summary>
/// Transaction type. Serialized as a string by the API ("Buy"/"Sell"/"Invalid").
/// </summary>
public enum TransactionType
{
    Buy,
    Sell,
    Invalid
}

/// <summary>
/// A buy/sell transaction. Mirrors the backend's TransactionDto.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public Guid HoldingId { get; set; }

    // Denormalized security info
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;

    public TransactionType TransactionType { get; set; }
    public decimal Shares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Fees { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Payload for creating a transaction.
/// Note: the API route already identifies the portfolio; the body identifies the holding.
/// </summary>
public class CreateTransactionRequest
{
    public Guid HoldingId { get; set; }
    public TransactionType TransactionType { get; set; } = TransactionType.Buy;
    public decimal Shares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal Fees { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

/// <summary>
/// Payload for updating an existing transaction.
/// </summary>
public class UpdateTransactionRequest
{
    public TransactionType TransactionType { get; set; }
    public decimal Shares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal Fees { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
}
