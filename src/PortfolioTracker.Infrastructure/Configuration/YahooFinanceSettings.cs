namespace PortfolioTracker.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Yahoo Finance API via RapidAPI.
/// </summary>
public class YahooFinanceSettings
{
    public string ApiKey { get; set; } = string.Empty;
    //public string BaseUrl { get; set; } = "https://query1.finance.yahoo.com/v7/finance";
    public string BaseUrl { get; set; } = "https://yahoo-finance166.p.rapidapi.com";
    public string RapidApiHost { get; set; } = "yahoo-finance166.p.rapidapi.com";
    public string Region { get; set; } = "US";
}
