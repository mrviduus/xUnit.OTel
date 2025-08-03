using Microsoft.Extensions.DependencyInjection;


// Define the namespace for test classes
namespace xUnit.OTel.Tests;

// Test class that uses the assembly fixture to demonstrate full OpenTelemetry integration
// This class shows how to use dependency injection to get instrumented HTTP clients
// All HTTP requests made through this client will be automatically traced
public class WebTests
{
    private readonly HttpClient _httpClient;

    public WebTests(TestSetup setup)
    {
        // Get the web caller from our setup
        var factory = setup.Host.Services.GetRequiredService<IHttpClientFactory>();
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task TestGoogleIsWorking()
    {
        // This will show you:
        // - When the call started
        // - How long it took
        // - If it worked or failed
        var response = await _httpClient.GetAsync("https://www.google.com", TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);
    }
}
