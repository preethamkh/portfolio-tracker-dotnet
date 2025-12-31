using System.Net.Http.Json;
using System.Text.Json;

namespace PortfolioTracker.IntegrationTests.Helpers
{
    /// <summary>
    /// Extension methods for HttpClient to simplify test requests.
    /// Makes tests more readable and reduces boilerplate.
    /// </summary>
    /// <remarks>
    /// Why extension methods?
    /// 1. Adds functionality to existing HttpClient class
    /// 2. Call like: await client.PostAsJsonAsync(...)
    /// 3. More readable than: await client.PostAsync(url, CreateJsonContent(...))
    /// 
    /// Pattern: Extension Methods
    /// - Static class with static methods
    /// - First parameter has 'this' keyword
    /// - Allows calling as if method was on the original class
    /// 
    /// Before extensions:
    /// var json = JsonSerializer.Serialize(dto);
    /// var content = new StringContent(json, Encoding.UTF8, "application/json");
    /// var response = await client.PostAsync(url, content);
    /// 
    /// With extensions:
    /// var response = await client.PostAsJsonAsync(url, dto);
    /// </remarks>
    public static class HttpClientExtensions
    {
        #region JSON Serialization Options

        /// <summary>
        /// JSON serialization options matching ASP.NET Core defaults.
        /// Ensures test serialization matches API serialization.
        /// </summary>
        /// <remarks>
        /// ASP.NET Core uses these settings by default:
        /// - PropertyNameCaseInsensitive: true (API accepts any case)
        /// - PropertyNamingPolicy: CamelCase (API returns camelCase)
        /// 
        /// Why match API settings?
        /// - Tests should behave like real clients
        /// - Catches serialization issues
        /// - Consistent behavior
        /// 
        /// Example difference:
        /// C# property: FullName
        /// JSON (camelCase): fullName
        /// JSON (default): FullName
        /// 
        /// API expects camelCase, so we must match!
        /// </remarks>
        public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        #endregion

        #region POST Extensions

        /// <summary>
        /// Sends a POST request with a JSON body.
        /// Automatically serializes the DTO to JSON using predefined options.
        /// </summary>
        /// <typeparam name="T">Type of object to send</typeparam>
        /// <param name="client">HTTP client</param>
        /// <param name="url">Request URL</param>
        /// <param name="dto">Data transfer object</param>
        /// <returns>HTTP response message</returns>
        /// <remarks>
        /// This replaces:
        /// var json = JsonSerializer.Serialize(dto);
        /// var content = new StringContent(json, Encoding.UTF8, "application/json");
        /// var response = await client.PostAsync(url, content);
        /// 
        /// With:
        /// var response = await client.PostAsJsonAsync(url, dto);
        /// 
        /// Example usage:
        /// var createDto = new CreateUserDto { Email = "test@test.com" };
        /// var response = await client.PostAsJsonAsync("/api/users", createDto);
        /// </remarks>
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string url, T dto)
        {
            // Uses System.Net.Http.Json extension that's built-in
            // We wrap it to apply our JsonOptions
            return client.PostAsJsonAsync(url, dto, JsonOptions);
        }

        #endregion

        #region PUT Extensions

        /// <summary>
        /// Sends PUT request with JSON body.
        /// </summary>
        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string url, T dto)
        {
            return client.PutAsJsonAsync(url, dto, JsonOptions);
        }

        #endregion
    }
}
