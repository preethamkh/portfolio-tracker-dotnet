using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Web.Interfaces.Services;
using PortfolioTracker.Web.Models.ViewModels.Dashboard;

namespace PortfolioTracker.Web.Controllers;

/// <summary>
/// Dashboard controller - landing page for authenticated users.
/// Requires user to be logged in via [Authorize] attribute.
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IApiClient apiClient, ILogger<DashboardController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// GET /Dashboard or /Dashboard/Index
    /// Main dashboard landing page - shows user welcome and portfolio summary
    /// </summary>
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";

        var portfolios = await _apiClient.GetPortfoliosAsync();

        var model = new DashboardViewModel
        {
            Portfolios = portfolios,
            TotalValue = portfolios.Sum(p => p.TotalValue),
            TotalGainLoss = portfolios.Sum(p => p.TotalGainLoss),
            TotalHoldings = portfolios.Sum(p => p.HoldingsCount),
            // TotalGainLossPercent calculated below
        };

        // Avoid divide by zero
        var totalCost = model.TotalValue - model.TotalGainLoss;
        model.TotalGainLossPercent = totalCost != 0
            ? (model.TotalGainLoss / totalCost) * 100
            : 0;

        return View(model);
    }
}
