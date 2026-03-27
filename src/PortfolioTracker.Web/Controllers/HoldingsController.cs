using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Web.Models.ViewModels.Holdings;
using PortfolioTracker.Web.Services;

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
}