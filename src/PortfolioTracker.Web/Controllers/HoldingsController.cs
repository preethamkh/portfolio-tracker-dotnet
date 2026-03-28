using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Web.Interfaces.Services;
using PortfolioTracker.Web.Models.ViewModels.Holdings;

namespace PortfolioTracker.Web.Controllers;

[Authorize]
public class HoldingsController : Controller
{
    private readonly IApiClient _apiClient;

    public HoldingsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET /Holdings?portfolioId=5
    public async Task<IActionResult> Index(int portfolioId)
    {
        ViewData["Title"] = "Holdings";
        ViewData["PortfolioId"] = portfolioId;

        var holdings = await _apiClient.GetHoldingsAsync(portfolioId);
        return View(holdings);
    }

    // GET /Holdings/Create?portfolioId=5
    public async Task<IActionResult> Create(int portfolioId)
    {
        ViewData["Title"] = "Add Holding";

        var portfolios = await _apiClient.GetPortfoliosAsync();
        var model = new CreateHoldingViewModel
        {
            PortfolioId = portfolioId,
            SelectedPortfolioId = portfolioId,
            AvailablePortfolios = portfolios.Select(p => new PortfolioSelectItem
            {
                Id = p.Id,
                Name = p.Name
            }).ToList()
        };

        return View(model);
    }

    // POST /Holdings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHoldingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Repopulate dropdowns before returning view
            var portfolios = await _apiClient.GetPortfoliosAsync();
            model.AvailablePortfolios = portfolios.Select(p => new PortfolioSelectItem
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();
            return View(model);
        }

        var result = await _apiClient.CreateHoldingAsync(model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to add holding. Check that the symbol is valid.");

            var portfolios = await _apiClient.GetPortfoliosAsync();
            model.AvailablePortfolios = portfolios.Select(p => new PortfolioSelectItem
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();
            return View(model);
        }

        TempData["Success"] = $"Holding {model.Symbol.ToUpper()} added successfully!";
        return RedirectToAction("Detail", "Portfolio", new { id = model.SelectedPortfolioId });
    }

    // POST /Holdings/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int portfolioId)
    {
        var success = await _apiClient.DeleteHoldingAsync(id);

        TempData[success ? "Success" : "Error"] = success
            ? "Holding removed."
            : "Failed to remove holding.";

        return RedirectToAction("Detail", "Portfolio", new { id = portfolioId });
    }
}