using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Web.Models.ViewModels.Holdings;

public class CreateHoldingViewModel
{
    public int PortfolioId { get; set; }

    [Required(ErrorMessage = "Please select a portfolio")]
    [Display(Name = "Portfolio")]
    public int SelectedPortfolioId { get; set; }

    [Required(ErrorMessage = "Stock symbol is required")]
    [StringLength(10, ErrorMessage = "Symbol cannot exceed 10 characters")]
    [Display(Name = "Stock Symbol")]
    public string Symbol { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.0001, 1000000, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Purchase price is required")]
    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Purchase Price Per Share")]
    public decimal PurchasePrice { get; set; }

    [Required(ErrorMessage = "Purchase date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Purchase Date")]
    public DateTime PurchaseDate { get; set; } = DateTime.Today;

    // Populated from API for the dropdown
    public List<PortfolioSelectItem> AvailablePortfolios { get; set; } = new();
}
