using Microsoft.Extensions.DependencyInjection;
using xUnit.OTel.Attributes;
using Xunit;

namespace xUnit.OTel.Tests;

// Simple test class without assembly fixture to test basic functionality
public class SimpleHttpClientTests
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // Basic test to verify test discovery is working
        Assert.True(true);
    }

    [Fact]
    [Trace]
    public async Task GetRequest_ToExample_ShouldWork()
    {
        // Create a simple HttpClient for testing
        using var httpClient = new HttpClient();

        // Make a simple request
        var response = await httpClient.GetAsync("https://httpbin.org/get", TestContext.Current.CancellationToken);

        // Basic assertions
        Assert.True(response.IsSuccessStatusCode);
    }
}

// Test class that uses the assembly fixture
public class HttpClientTests
{
    private readonly HttpClient _httpClient;

    public HttpClientTests(TestFixture fixture)
    {

        // Get HttpClient from DI container (this will be instrumented)
        var httpClientFactory = fixture.Host.Services.GetRequiredService<IHttpClientFactory>();
        _httpClient = httpClientFactory.CreateClient();

    }

    [Fact]
    [Trace]
    public async Task GetRequest_ToJsonPlaceholder_ShouldLogHttpActivity()
    {
        // Arrange
        const string url = "https://jsonplaceholder.typicode.com/posts/1";

        // Act

        var response = await _httpClient.GetAsync(url, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotEmpty(content);
    }

    [Fact]
    [Trace]
    public async Task PostRequest_ToJsonPlaceholder_ShouldLogHttpActivity()
    {
        // Arrange
        const string url = "https://jsonplaceholder.typicode.com/posts";
        const string jsonPayload = """
            {
                "title": "Test Post from xUnit.OTel",
                "body": "This is a test post to verify HTTP instrumentation in OpenTelemetry",
                "userId": 1
            }
            """;

        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content, TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotEmpty(responseContent);
    }

    [Fact]
    [Trace]
    public async Task MultipleRequests_ShouldGenerateMultipleSpans()
    {

        var urls = new[]
        {
            "https://jsonplaceholder.typicode.com/posts/1",
            "https://jsonplaceholder.typicode.com/posts/2",
            "https://jsonplaceholder.typicode.com/users/1"
        };


        // Act - Make multiple concurrent requests
        var tasks = urls.Select(async url =>
        {
            var response = await _httpClient.GetAsync(url, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            return new { Url = url, StatusCode = response.StatusCode, ContentLength = content.Length };
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result =>
        {
            Assert.True(result.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.True(result.ContentLength > 0);
        });


    }

    [Fact]
    [Trace]
    public async Task ErrorRequest_ShouldLogHttpErrorActivity()
    {
        // Arrange - Use a more reliable error endpoint
        const string invalidUrl = "https://httpbin.org/status/500"; // This returns HTTP 500

        // Act & Assert

        var response = await _httpClient.GetAsync(invalidUrl, TestContext.Current.CancellationToken);

        // Verify we get the expected 500 status
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
