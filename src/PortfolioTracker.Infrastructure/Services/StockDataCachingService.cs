using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Infrastructure.Configuration;

namespace PortfolioTracker.Infrastructure.Services;

/// <summary>
/// Decorator that adds Redis caching to any IStockDataService implementation.
/// Implements the Decorator Pattern to wrap another IStockDataService.
/// </summary>
/// <remarks>
/// DECORATOR PATTERN EXPLANATION:
/// - This class "decorates" (wraps) another IStockDataService
/// - It implements the same interface (IStockDataService)
/// - Adds caching behavior without modifying the wrapped service
/// 
/// Flow:
/// 1. Check Redis cache
/// 2. If found return cached data (fast path)
/// 3. If not found call wrapped service (slow path)
/// 4. Store result in cache for next time
/// 5. Return result
/// 
/// Why this pattern?
/// - AlphaVantageService doesn't know it's being cached
/// - Can wrap ANY IStockDataService implementation
/// - Easy to add/remove caching by changing DI registration
/// - Single Responsibility: Caching is separate from API calls
/// </remarks>
public class StockDataCachingService : IStockDataService
{
    private readonly IStockDataService _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<StockDataCachingService> _logger;
    private readonly StockDataCacheSettings _cacheSettings;

    // Cache key prefixes for different data types
    private const string QuoteKeyPrefix = "quote:";
    private const string CompanyKeyPrefix = "company:";
    private const string SearchKeyPrefix = "search:";
    private const string HistoricalKeyPrefix = "historical:";

    /// <summary>
    /// Constructor using Decorator Pattern.
    /// </summary>
    /// <param name="innerService">The wrapped service (e.g., AlphaVantageService)</param>
    /// <param name="cache">IDistributedCache - abstraction over Redis</param>
    /// <param name="logger">Logger for this caching service</param>
    /// <param name="cacheSettings">Cache duration configuration</param>
    public StockDataCachingService(
        IStockDataService innerService,
        IDistributedCache cache,
        ILogger<StockDataCachingService> logger,
        IOptions<StockDataCacheSettings> cacheSettings)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
    {
        var cacheKey = $"{QuoteKeyPrefix}{symbol.ToUpperInvariant()}";

        // Try to get from cache first
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            _logger.LogDebug("Cache HIT for quote: {Symbol}", symbol);
            return JsonSerializer.Deserialize<StockQuoteDto>(cachedData);
        }

        _logger.LogDebug("Cache MISS for quote: {Symbol}, fetching from API", symbol);

        // Cache miss - call the wrapped service
        var quote = await _innerService.GetQuoteAsync(symbol);

        if (quote != null)
        {
            // Store in cache with expiration
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                    _cacheSettings.QuoteCacheDurationMinutes)
            };

            var serialized = JsonSerializer.Serialize(quote);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            _logger.LogDebug(
                "Cached quote for {Symbol}, expires in {Minutes} minutes",
                symbol,
                _cacheSettings.QuoteCacheDurationMinutes);
        }

        return quote;
    }

    public async Task<CompanyInfoDto?> GetCompanyInfoAsync(string symbol)
    {
        var cacheKey = $"{CompanyKeyPrefix}{symbol.ToUpperInvariant()}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            _logger.LogDebug("Cache HIT for company info: {Symbol}", symbol);
            return JsonSerializer.Deserialize<CompanyInfoDto>(cachedData);
        }

        _logger.LogDebug("Cache MISS for company info: {Symbol}, fetching from API", symbol);

        var companyInfo = await _innerService.GetCompanyInfoAsync(symbol);

        if (companyInfo != null)
        {
            // Company info changes rarely - cache for 30 days
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(
                    _cacheSettings.CompanyInfoCacheDurationDays)
            };

            var serialized = JsonSerializer.Serialize(companyInfo);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            _logger.LogDebug(
                "Cached company info for {Symbol}, expires in {Days} days",
                symbol,
                _cacheSettings.CompanyInfoCacheDurationDays);
        }

        return companyInfo;
    }

    public async Task<List<ExternalSecuritySearchDto>> SearchSecuritiesAsync(string query, int limit = 10)
    {
        // Create cache key from query and limit
        // Note: We normalize the query (uppercase, trim) to improve cache hit rate
        var normalizedQuery = query.Trim().ToUpperInvariant();
        var cacheKey = $"{SearchKeyPrefix}{normalizedQuery}:{limit}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            _logger.LogDebug("Cache HIT for search: {Query}", query);
            return JsonSerializer.Deserialize<List<ExternalSecuritySearchDto>>(cachedData)
                   ?? new List<ExternalSecuritySearchDto>();
        }

        _logger.LogDebug("Cache MISS for search: {Query}, fetching from API", query);

        var results = await _innerService.SearchSecuritiesAsync(query, limit);

        if (results.Any())
        {
            // Cache search results for 1 day
            // Search results don't change often (company listings are relatively stable)
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            };

            var serialized = JsonSerializer.Serialize(results);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            _logger.LogDebug("Cached search results for query: {Query}", query);
        }

        return results;
    }

    public async Task<List<HistoricalPriceDto>?> GetHistoricalPricesAsync(string symbol,
        DateTime startDate,
        DateTime endDate)
    {
        // Cache key includes date range
        var cacheKey = $"{HistoricalKeyPrefix}{symbol.ToUpperInvariant()}:" +
                      $"{startDate:yyyyMMdd}-{endDate:yyyyMMdd}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            _logger.LogDebug(
                "Cache HIT for historical data: {Symbol} ({StartDate} to {EndDate})",
                symbol, startDate, endDate);
            return JsonSerializer.Deserialize<List<HistoricalPriceDto>>(cachedData);
        }

        _logger.LogDebug(
            "Cache MISS for historical data: {Symbol} ({StartDate} to {EndDate}), fetching from API",
            symbol, startDate, endDate);

        var historicalData = await _innerService.GetHistoricalPricesAsync(symbol, startDate, endDate);

        if (historicalData != null && historicalData.Any())
        {
            // Historical data doesn't change (the past is fixed)
            // But we cache for shorter time in case user wants updated data with new days added
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(
                    _cacheSettings.HistoricalDataCacheDurationDays)
            };

            var serialized = JsonSerializer.Serialize(historicalData);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            _logger.LogDebug(
                "Cached historical data for {Symbol}, expires in {Days} day(s)",
                symbol,
                _cacheSettings.HistoricalDataCacheDurationDays);
        }

        return historicalData;
    }
}


// UNDERSTANDING IDistributedCache

// IDistributedCache is Microsoft's abstraction for distributed caching.
// It provides simple get/set operations that work with multiple backends:
//
// - Redis (what we're using)
// - SQL Server
// - NCache
// - In-memory (for development)
//
// Main methods:
// - GetStringAsync(key) returns cached string or null
// - SetStringAsync(key, value, options) stores string with expiration
// - RemoveAsync(key) deletes from cache
// - RefreshAsync(key) resets expiration timer
//
// Why use this instead of Redis-specific library?
// - Provider agnostic (can switch from Redis to SQL Server cache)
// - Built into ASP.NET Core
// - Simple API for common scenarios
// - Automatic serialization with extension methods
//
// For advanced Redis features (pub/sub, sorted sets, etc.), you'd use
// StackExchange.Redis directly instead of IDistributedCache.