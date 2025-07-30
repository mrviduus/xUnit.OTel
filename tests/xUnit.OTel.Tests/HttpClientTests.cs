using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using xUnit.OTel.Attributes;
using Xunit;
using Xunit.v3;

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
    private readonly ITestOutputHelper _output;
    private readonly ILogger<HttpClientTests> _logger;
    private readonly HttpClient _httpClient;

    public HttpClientTests(ITestOutputHelper output, TestFixture fixture)
    {
        _output = output;
        
        // Get logger from the test fixture's host
        _logger = fixture.Host.Services.GetRequiredService<ILogger<HttpClientTests>>();
        
        // Get HttpClient from DI container (this will be instrumented)
        var httpClientFactory = fixture.Host.Services.GetRequiredService<IHttpClientFactory>();
        _httpClient = httpClientFactory.CreateClient();
        
        _output.WriteLine("HttpClientTests initialized with instrumented HttpClient and Logger");
    }

    [Fact]
    [Trace]
    public async Task GetRequest_ToJsonPlaceholder_ShouldLogHttpActivity()
    {
        // Arrange
        const string url = "https://jsonplaceholder.typicode.com/posts/1";
        _logger.LogInformation("Starting HTTP GET request to {Url}", url);

        // Act
        _output.WriteLine($"Making GET request to: {url}");
        
        var response = await _httpClient.GetAsync(url, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotEmpty(content);
        
        _logger.LogInformation("HTTP GET request completed successfully. Status: {StatusCode}, Content Length: {ContentLength}", 
            response.StatusCode, content.Length);
        
        _output.WriteLine($"Response Status: {response.StatusCode}");
        _output.WriteLine($"Content Length: {content.Length} characters");
        _output.WriteLine("First 200 characters of response:");
        _output.WriteLine(content.Length > 200 ? content[..200] + "..." : content);
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
        
        _logger.LogInformation("Starting HTTP POST request to {Url}", url);

        // Act
        _output.WriteLine($"Making POST request to: {url}");
        _output.WriteLine($"Payload: {jsonPayload}");
        
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content, TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotEmpty(responseContent);
        
        _logger.LogInformation("HTTP POST request completed successfully. Status: {StatusCode}, Response Length: {ResponseLength}", 
            response.StatusCode, responseContent.Length);
        
        _output.WriteLine($"Response Status: {response.StatusCode}");
        _output.WriteLine($"Response Content: {responseContent}");
    }

    [Fact]
    [Trace]
    public async Task MultipleRequests_ShouldGenerateMultipleSpans()
    {
        _logger.LogInformation("Starting multiple HTTP requests test");

        var urls = new[]
        {
            "https://jsonplaceholder.typicode.com/posts/1",
            "https://jsonplaceholder.typicode.com/posts/2",
            "https://jsonplaceholder.typicode.com/users/1"
        };

        _output.WriteLine($"Making {urls.Length} concurrent HTTP requests...");

        // Act - Make multiple concurrent requests
        var tasks = urls.Select(async url =>
        {
            _logger.LogInformation("Making request to {Url}", url);
            var response = await _httpClient.GetAsync(url, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            
            _logger.LogInformation("Completed request to {Url} with status {StatusCode}", url, response.StatusCode);
            return new { Url = url, StatusCode = response.StatusCode, ContentLength = content.Length };
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result =>
        {
            Assert.True(result.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.True(result.ContentLength > 0);
        });

        foreach (var result in results)
        {
            _output.WriteLine($"✓ {result.Url} - Status: {result.StatusCode}, Length: {result.ContentLength}");
        }

        _logger.LogInformation("All {Count} HTTP requests completed successfully", results.Length);
    }

    [Fact]
    [Trace]
    public async Task ErrorRequest_ShouldLogHttpErrorActivity()
    {
        // Arrange - Use a more reliable error endpoint
        const string invalidUrl = "https://httpbin.org/status/500"; // This returns HTTP 500
        _logger.LogInformation("Starting HTTP request to error endpoint {Url}", invalidUrl);

        // Act & Assert
        _output.WriteLine($"Making GET request to error endpoint: {invalidUrl}");
        
        var response = await _httpClient.GetAsync(invalidUrl, TestContext.Current.CancellationToken);
        
        _logger.LogWarning("HTTP request returned error status: {StatusCode}", response.StatusCode);
        _output.WriteLine($"Response Status: {response.StatusCode}");
        
        // Verify we get the expected 500 status
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
