namespace PortfolioTracker.Web.Models.ViewModels.Securities;

public class SecuritySearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<SecurityViewModel> Results { get; set; } = new();
    public bool HasSearched { get; set; }
}
