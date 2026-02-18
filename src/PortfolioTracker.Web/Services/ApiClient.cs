using PortfolioTracker.Web.Interfaces.Services;
using PortfolioTracker.Web.Models.ViewModels.Auth;
using PortfolioTracker.Web.Models.ViewModels.Holdings;
using PortfolioTracker.Web.Models.ViewModels.Portfolio;
using PortfolioTracker.Web.Models.ViewModels.Securities;
using PortfolioTracker.Web.Models.ViewModels.Transactions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PortfolioTracker.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient, ITokenService tokenService, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private void AttachAuthHeader()
    {
        var token = _tokenService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<T?> GetAsync<T>(string endpoint)
    {
        AttachAuthHeader();

        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GET {Endpoint}", endpoint);
            return default;
        }
    }

    private async Task<T?> PostAsync<T>(string endpoint, object body)
    {
        AttachAuthHeader();

        try
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("POST {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                return default;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during POST {Endpoint}", endpoint);
            return default;
        }
    }

    private async Task<bool> DeleteAsync(string endpoint)
    {
        AttachAuthHeader();

        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DELETE {Endpoint}", endpoint);
            return false;
        }
    }

    // -----------------------------------------------------------------------
    // Auth
    // -----------------------------------------------------------------------

    public async Task<AuthResponseDto?> LoginAsync(LoginViewModel model)
    {
        return await PostAsync<AuthResponseDto>("api/auth/login", new
        {
            email = model.Email,
            password = model.Password
        });
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterViewModel model)
    {
        return await PostAsync<AuthResponseDto>("api/auth/register", new
        {
            username = model.Username,
            email = model.Email,
            password = model.Password
        });
    }

    // -----------------------------------------------------------------------
    // Portfolios
    // -----------------------------------------------------------------------

    public async Task<List<PortfolioViewModel>> GetPortfoliosAsync()
    {
        return await GetAsync<List<PortfolioViewModel>>("api/portfolios")
               ?? new List<PortfolioViewModel>();
    }

    public async Task<PortfolioViewModel?> GetPortfolioAsync(int id)
    {
        return await GetAsync<PortfolioViewModel>($"api/portfolios/{id}");
    }

    public async Task<PortfolioViewModel?> CreatePortfolioAsync(CreatePortfolioViewModel model)
    {
        return await PostAsync<PortfolioViewModel>("api/portfolios", new
        {
            name = model.Name,
            description = model.Description
        });
    }

    public async Task<bool> DeletePortfolioAsync(int id)
    {
        return await DeleteAsync($"api/portfolios/{id}");
    }

    // -----------------------------------------------------------------------
    // Holdings
    // -----------------------------------------------------------------------

    public async Task<List<HoldingViewModel>> GetHoldingsAsync(int portfolioId)
    {
        return await GetAsync<List<HoldingViewModel>>($"api/portfolios/{portfolioId}/holdings")
               ?? new List<HoldingViewModel>();
    }

    public async Task<HoldingViewModel?> GetHoldingAsync(int id)
    {
        return await GetAsync<HoldingViewModel>($"api/holdings/{id}");
    }

    public async Task<HoldingViewModel?> CreateHoldingAsync(CreateHoldingViewModel model)
    {
        return await PostAsync<HoldingViewModel>($"api/portfolios/{model.SelectedPortfolioId}/holdings", new
        {
            symbol = model.Symbol.ToUpper(),
            quantity = model.Quantity,
            purchasePrice = model.PurchasePrice,
            purchaseDate = model.PurchaseDate
        });
    }

    public async Task<bool> DeleteHoldingAsync(int id)
    {
        return await DeleteAsync($"api/holdings/{id}");
    }

    // -----------------------------------------------------------------------
    // Transactions
    // -----------------------------------------------------------------------

    public async Task<List<TransactionViewModel>> GetTransactionsAsync(int holdingId)
    {
        return await GetAsync<List<TransactionViewModel>>($"api/holdings/{holdingId}/transactions")
               ?? new List<TransactionViewModel>();
    }

    public async Task<TransactionViewModel?> CreateTransactionAsync(CreateTransactionViewModel model)
    {
        return await PostAsync<TransactionViewModel>($"api/holdings/{model.HoldingId}/transactions", new
        {
            transactionType = model.TransactionType,
            quantity = model.Quantity,
            price = model.Price,
            fees = model.Fees,
            transactionDate = model.TransactionDate,
            notes = model.Notes
        });
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        return await DeleteAsync($"api/transactions/{id}");
    }

    // -----------------------------------------------------------------------
    // Securities
    // -----------------------------------------------------------------------

    public async Task<List<SecurityViewModel>> SearchSecuritiesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<SecurityViewModel>();

        return await GetAsync<List<SecurityViewModel>>(
                   $"api/securities/search?query={Uri.EscapeDataString(query)}")
               ?? new List<SecurityViewModel>();
    }

    public async Task<SecurityViewModel?> GetSecurityAsync(string symbol)
    {
        return await GetAsync<SecurityViewModel>($"api/securities/{symbol.ToUpper()}");
    }
}
