using System.Net.Http.Json;
using System.Text.Json;

namespace PortfolioTracker.IntegrationTests.Helpers;

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

    /// <summary>
    /// Sends PUT request with JSON body.
    /// </summary>
    public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string url, T dto)
    {
        return client.PutAsJsonAsync(url, dto, JsonOptions);
    }

    /// <summary>
    /// Reads response body as JSON and deserializes to specified type.
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="response">HTTP response    </param>
    /// <returns>Deserialized object</returns>
    /// <remarks>
    /// This replaces:
    /// var content = await response.Content.ReadAsStringAsync();
    ///  return JsonSerializer.Deserialize<T/>(content, JsonOptions);
    /// With:
    /// var user = await response.ReadAsJsonAsync<T/>();
    /// 
    /// What happens:
    /// 1. Reads response body as string
    /// 2. Deserializes JSON to specified type
    /// 3. Returns typed object
    /// 
    /// Example:
    /// var response = await client.GetAsync("/api/users/123");
    /// var user = await response.ReadAsJsonAsync<T/>();
    /// 
    /// user.Email.Should().Be("test@test.com"); // Strongly typed
    /// </remarks>
    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);

        // alternative without built-in extension:
        //var content = await response.Content.ReadAsStringAsync();
        //return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Reads response and asserts it's successful, then deserializes.
    /// Convenience method combining success check and deserialization.
    /// </summary>
    public static async Task<T> ReadAsSuccessfulJsonAsync<T>(this HttpResponseMessage response)
    {
        // throws exception if not 2xx
        response.EnsureSuccessStatusCode();

        var result = await response.ReadAsJsonAsync<T>();
        // null forgiving because EnsureSuccessStatusCode ensures valid content and would have thrown otherwise
        return result!;
    }

    /// <summary>
    /// Reads response content as string.
    /// Useful for asserting raw response bodies in tests.
    /// </summary>
    /// <remarks>
    /// This helps you test error messages:
    /// 
    /// var response = await client.PostAsync(...);
    /// response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    /// 
    /// var error = await response.ReadAsStringAsync();
    /// error.Should().Contain("already exists");
    /// </remarks>
    public static async Task<string> ReadAsStringAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Builds URL with query parameters.
    /// </summary>
    public static string BuildUrl(string baseUrl, Dictionary<string, string>? queryParams = null)
    {
        if (queryParams == null || queryParams.Count == 0)
        {
            return baseUrl;
        }

        // Uri.EscapeDataString encodes special characters
        var queryString = string.Join("&",
            queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            
        return $"{baseUrl}?{queryString}";
    }
}