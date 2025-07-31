// Import Microsoft dependency injection services for accessing configured services
// Import custom trace attribute for adding OpenTelemetry instrumentation to tests
// Import xUnit framework for test definitions and assertions

// Define the namespace for test classes
namespace xUnit.OTel.Tests;

// Test class that uses the assembly fixture to demonstrate full OpenTelemetry integration
// This class shows how to use dependency injection to get instrumented HTTP clients
// All HTTP requests made through this client will be automatically traced
public class HttpClientTests(TestFixture fixture)
{
    // Private field to store the instrumented HTTP client for use in test methods
    // This client is configured with OpenTelemetry instrumentation through dependency injection
    private readonly HttpClient _httpClient = fixture.GetRequiredService<IHttpClientFactory>().CreateClient();


    // Test method that demonstrates HTTP GET request tracing with OpenTelemetry
    [Fact]
    public async Task GetRequest_ToJsonPlaceholder_ShouldLogHttpActivity()
    {
        // Arrange - Set up the test data and expected conditions
        // Use a reliable test API endpoint that returns predictable JSON data
        const string url = "https://jsonplaceholder.typicode.com/posts/1";

        // Act - Execute the operation being tested
        // Make an HTTP GET request using the instrumented client
        var response = await _httpClient.GetAsync(url, TestContext.Current.CancellationToken);
        // Read the response content to complete the HTTP operation
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert - Verify the results meet expectations
        // Ensure the HTTP request was successful (throws exception if not 2xx status)
        response.EnsureSuccessStatusCode();
        // Verify that we received actual content in the response
        Assert.NotEmpty(content);
    }

    // Test method that demonstrates HTTP POST request tracing with JSON payload
    // This test shows how OpenTelemetry captures both request and response details
    [Fact]
    public async Task PostRequest_ToJsonPlaceholder_ShouldLogHttpActivity()
    {
        // Arrange - Prepare test data for the POST request
        // Use a test API endpoint that accepts POST requests
        const string url = "https://jsonplaceholder.typicode.com/posts";

        // Define JSON payload using raw string literal for better readability
        // This creates a realistic test payload with structured data
        const string jsonPayload = """
            {
                "title": "Test Post from xUnit.OTel",
                "body": "This is a test post to verify HTTP instrumentation in OpenTelemetry",
                "userId": 1
            }
            """;

        // Create HTTP content with proper JSON formatting and content type
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        // Act - Execute the POST request with the JSON payload
        var response = await _httpClient.PostAsync(url, content, TestContext.Current.CancellationToken);
        // Read the response content to verify the server processed the request
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert - Verify the POST request was successful
        // Ensure the HTTP request was successful (throws exception if not 2xx status)
        response.EnsureSuccessStatusCode();
        // Verify that the server returned actual response content
        Assert.NotEmpty(responseContent);
    }

    // Test method that demonstrates concurrent HTTP requests and span correlation
    // This test shows how OpenTelemetry creates separate spans for concurrent operations
    [Fact]
    public async Task MultipleRequests_ShouldGenerateMultipleSpans()
    {

        // Arrange - Define multiple test URLs for concurrent requests
        // Use different endpoints to create distinct HTTP activities in the trace
        var urls = new[]
        {
            "https://jsonplaceholder.typicode.com/posts/1",
            "https://jsonplaceholder.typicode.com/posts/2",
            "https://jsonplaceholder.typicode.com/users/1"
        };


        // Act - Make multiple concurrent requests using async operations
        // Create a task for each URL that performs an HTTP GET request
        var tasks = urls.Select(async url =>
        {
            // Make the HTTP request and read the response content
            var response = await _httpClient.GetAsync(url, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Return an anonymous object with request details for verification
            return new { Url = url, StatusCode = response.StatusCode, ContentLength = content.Length };
        });

        // Wait for all concurrent requests to complete
        var results = await Task.WhenAll(tasks);

        // Assert - Verify all requests completed successfully
        // Check that each request returned a successful status code and content
        Assert.All(results, result =>
        {
            // Verify each request returned HTTP 200 OK status
            Assert.True(result.StatusCode == System.Net.HttpStatusCode.OK);
            // Verify each response contained actual content (not empty)
            Assert.True(result.ContentLength > 0);
        });


    }

    // Test method that demonstrates error handling and tracing of failed HTTP requests
    // This test shows how OpenTelemetry captures and traces HTTP error scenarios
    [Fact]
    public async Task ErrorRequest_ShouldLogHttpErrorActivity()
    {
        // Arrange - Use a more reliable error endpoint
        // Use an endpoint that predictably returns HTTP 500 for testing error scenarios
        const string invalidUrl = "https://httpbin.org/status/500"; // This returns HTTP 500

        // Act & Assert - Execute the request and verify the expected error response
        // Make the HTTP request to the error endpoint
        var response = await _httpClient.GetAsync(invalidUrl, TestContext.Current.CancellationToken);

        // Verify we get the expected 500 status - this demonstrates error tracing
        // OpenTelemetry will capture this as an error condition in the HTTP span
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
