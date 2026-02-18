using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Web.Models.ViewModels.Transactions;

public class CreateTransactionViewModel
{
    public int HoldingId { get; set; }
    public string Symbol { get; set; } = string.Empty;

    [Required(ErrorMessage = "Transaction type is required")]
    [Display(Name = "Transaction Type")]
    public string TransactionType { get; set; } = "Buy";

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.0001, 1000000, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Price Per Share")]
    public decimal Price { get; set; }

    [Range(0, 10000, ErrorMessage = "Fees cannot be negative")]
    [Display(Name = "Brokerage Fees (optional)")]
    public decimal Fees { get; set; }

    [Required(ErrorMessage = "Date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Transaction Date")]
    public DateTime TransactionDate { get; set; } = DateTime.Today;

    [StringLength(500)]
    public string? Notes { get; set; }
}
