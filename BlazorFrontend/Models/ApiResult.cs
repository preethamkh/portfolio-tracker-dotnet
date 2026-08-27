namespace BlazorFrontend.Models;

/// <summary>
/// Generic result wrapper for API calls.
/// Keeps error handling explicit and consistent across services so pages
/// can render meaningful messages instead of throwing.
/// </summary>
public class ApiResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }
    public System.Net.HttpStatusCode StatusCode { get; private init; }

    /// <summary>
    /// True when the API returned 401/403 (used to trigger re-login).
    /// </summary>
    public bool IsUnauthorized =>
        StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden;

    public static ApiResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value, StatusCode = System.Net.HttpStatusCode.OK };

    public static ApiResult<T> Failure(string error, System.Net.HttpStatusCode statusCode) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}
