using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Web.Interfaces.Services;
using PortfolioTracker.Web.Models.ViewModels.Portfolio;

namespace PortfolioTracker.Web.Controllers;

/// <summary>
/// Portfolio controller - manages user portfolios.
/// Requires user to be logged in via [Authorize] attribute.
/// Used to test that unauthorized users get redirected to login.
/// </summary>
[Authorize]
public class PortfolioController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(IApiClient apiClient, ILogger<PortfolioController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// GET /Portfolio or /Portfolio/Index
    /// List all portfolios for the logged-in user.
    /// If not authenticated, should redirect to /Auth/Login
    /// </summary>
    // GET /Portfolio
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Portfolios";
        var portfolios = await _apiClient.GetPortfoliosAsync();
        return View(portfolios);
    }

    // GET /Portfolio/Create
    public IActionResult Create()
    {
        ViewData["Title"] = "New Portfolio";
        return View(new CreatePortfolioViewModel());
    }

    // POST /Portfolio/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePortfolioViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _apiClient.CreatePortfolioAsync(model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create portfolio. Please try again.");
            return View(model);
        }

        TempData["Success"] = $"Portfolio '{result.Name}' created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET /Portfolio/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        var portfolio = await _apiClient.GetPortfolioAsync(id);

        if (portfolio == null)
        {
            TempData["Error"] = "Portfolio not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = portfolio.Name;

        var model = new PortfolioDetailViewModel
        {
            Portfolio = portfolio,
            Holdings = await _apiClient.GetHoldingsAsync(id)
        };

        return View(model);
    }

    // POST /Portfolio/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _apiClient.DeletePortfolioAsync(id);

        TempData[success ? "Success" : "Error"] = success
            ? "Portfolio deleted."
            : "Failed to delete portfolio.";

        return RedirectToAction(nameof(Index));
    }
}
