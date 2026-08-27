using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorFrontend.Models;
using BlazorFrontend.Services.Interfaces;

namespace BlazorFrontend.Services;

/// <summary>
/// Concrete HTTP facade for the Portfolio Tracker API.
/// Adds the JWT bearer header per-request (thread-safe, unlike mutating
/// DefaultRequestHeaders) and normalizes error payloads into ApiResult.
/// </summary>
public class ApiClient(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ApiClient> logger) : IApiClient
{
    /// <summary>
    /// Shared serializer options. Matches the API's contract:
    /// camelCase property names and enums serialized as strings.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ApiResult<T>> GetAsync<T>(string url) =>
        await SendAsync<T>(HttpMethod.Get, url, body: null);

    public async Task<ApiResult<T>> PostAsync<T>(string url, object body) =>
        await SendAsync<T>(HttpMethod.Post, url, body);

    public async Task<ApiResult<T>> PutAsync<T>(string url, object body) =>
        await SendAsync<T>(HttpMethod.Put, url, body);

    public async Task<ApiResult<T>> DeleteAsync<T>(string url) =>
        await SendAsync<T>(HttpMethod.Delete, url, body: null);

    // ------------------------------------------------------------------
    // Core pipeline
    // ------------------------------------------------------------------

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);

            // Attach the JWT if the user is signed in
            var token = await tokenProvider.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (body is not null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            using var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // 204 No Content (e.g., DELETE) has no body to deserialize
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return ApiResult<T>.Success(default!);
                }

                var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                return value is null
                    ? ApiResult<T>.Failure("Empty response from server", response.StatusCode)
                    : ApiResult<T>.Success(value);
            }

            var error = await ExtractErrorAsync(response);
            logger.LogWarning("{Method} {Url} returned {StatusCode}: {Error}",
                method, url, (int)response.StatusCode, error);

            return ApiResult<T>.Failure(error, response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error calling {Method} {Url}", method, url);
            return ApiResult<T>.Failure(
                "Could not reach the server. Check your connection and try again.",
                System.Net.HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Request timed out calling {Method} {Url}", method, url);
            return ApiResult<T>.Failure("The request timed out. Please try again.",
                System.Net.HttpStatusCode.RequestTimeout);
        }
    }

    /// <summary>
    /// The API returns errors as { "message": "..." }. Fall back to a
    /// generic message for unexpected shapes (e.g., HTML error pages).
    /// </summary>
    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorPayload>(JsonOptions);
            if (!string.IsNullOrWhiteSpace(payload?.Message))
            {
                return payload.Message;
            }
        }
        catch (JsonException)
        {
            // Non-JSON error body - use the status-code based message below
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Your session has expired. Please sign in again.",
            System.Net.HttpStatusCode.Forbidden => "You don't have permission to perform this action.",
            System.Net.HttpStatusCode.NotFound => "The requested item was not found.",
            System.Net.HttpStatusCode.BadRequest => "The request was invalid. Please check your input.",
            _ => "Something went wrong. Please try again."
        };
    }

    private sealed record ErrorPayload(string? Message);
}
