using System.Net;
using System.Text;
using BlazorFrontend.Services;
using BlazorFrontend.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace PortfolioTracker.UnitTests.Blazor;

/// <summary>
/// HTTP handler stub that records requests and returns canned responses,
/// letting us test ApiClient's auth header and error mapping behavior.
/// </summary>
public class StubHttpHandler(HttpStatusCode statusCode, string responseJson) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];
    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (request.Content is not null)
        {
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
    }
}

/// <summary>
/// ApiClient behavior: bearer token attachment, JSON mapping and error
/// normalization into ApiResult.
/// </summary>
public class ApiClientTests
{
    private static ApiClient CreateClient(StubHttpHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7001/") };
        var tokenProvider = new Mock<ITokenProvider>();
        tokenProvider.Setup(p => p.GetTokenAsync()).ReturnsAsync("jwt-token");
        return new ApiClient(httpClient, tokenProvider.Object, NullLogger<ApiClient>.Instance);
    }

    [Fact]
    public async Task GetAsync_AttachesBearerToken()
    {
        // Arrange
        var handler = new StubHttpHandler(HttpStatusCode.OK, """{"id":"9d5e9b1e-6f4e-4c9a-9a5b-3d3c3f9f1a02","displayName":"Test"}""");
        var client = CreateClient(handler);

        // Act
        await client.GetAsync<DummyDto>("api/test");

        // Assert
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Headers.Authorization.Should().NotBeNull();
        handler.Requests[0].Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.Requests[0].Headers.Authorization!.Parameter.Should().Be("jwt-token");
    }

    [Fact]
    public async Task GetAsync_MapsCamelCaseJson_ToModel()
    {
        // Arrange
        var handler = new StubHttpHandler(HttpStatusCode.OK,
            """{"id":"9d5e9b1e-6f4e-4c9a-9a5b-3d3c3f9f1a01","displayName":"Test"}""");
        var client = CreateClient(handler);

        // Act
        var result = await client.GetAsync<DummyDto>("api/test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.DisplayName.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_ExtractsServerErrorMessage_OnBadRequest()
    {
        // Arrange
        var handler = new StubHttpHandler(HttpStatusCode.BadRequest, """{"message":"Name is required"}""");
        var client = CreateClient(handler);

        // Act
        var result = await client.GetAsync<DummyDto>("api/test");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Name is required");
    }

    [Fact]
    public async Task GetAsync_FlagsUnauthorizedResponses()
    {
        // Arrange
        var handler = new StubHttpHandler(HttpStatusCode.Unauthorized, """{"message":"Expired"}""");
        var client = CreateClient(handler);

        // Act
        var result = await client.GetAsync<DummyDto>("api/test");

        // Assert
        result.IsUnauthorized.Should().BeTrue();
    }

    [Fact]
    public async Task PostAsync_SerializesBodyAsJson()
    {
        // Arrange
        var handler = new StubHttpHandler(HttpStatusCode.Created, """{"id":"9d5e9b1e-6f4e-4c9a-9a5b-3d3c3f9f1a03","displayName":"x"}""");
        var client = CreateClient(handler);

        // Act
        await client.PostAsync<DummyDto>("api/test", new DummyDto { DisplayName = "x" });

        // Assert
        handler.LastRequestBody.Should().Contain("\"displayName\"");
    }

    [Fact]
    public async Task DeleteAsync_SucceedsOnNoContent()
    {
        // Arrange - API returns 204 with an empty body
        var handler = new StubHttpHandler(HttpStatusCode.NoContent, "");
        var client = CreateClient(handler);

        // Act
        var result = await client.DeleteAsync<Unit>("api/test/1");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private sealed record DummyDto
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; } = string.Empty;
    }
}
