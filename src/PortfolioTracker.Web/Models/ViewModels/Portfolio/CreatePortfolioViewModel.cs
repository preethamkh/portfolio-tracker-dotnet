using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Web.Models.ViewModels.Portfolio;

public class CreatePortfolioViewModel
{
    [Required(ErrorMessage = "Portfolio name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    [Display(Name = "Portfolio Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
}
