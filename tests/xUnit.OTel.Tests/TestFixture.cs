// Import Microsoft dependency injection services for service configuration
using Microsoft.Extensions.DependencyInjection;
// Import Microsoft hosting framework for application lifetime management
using Microsoft.Extensions.Hosting;
// Import Microsoft logging framework for logging services
using Microsoft.Extensions.Logging;
// Import custom OpenTelemetry diagnostics extensions
using xUnit.OTel.Diagnostics;
// Import xUnit framework for test infrastructure
using Xunit;

// Assembly-level attribute that registers this test fixture to be shared across all tests in the assembly
// This ensures that the OpenTelemetry configuration is set up once and reused for all tests
[assembly: AssemblyFixture(typeof(xUnit.OTel.Tests.TestFixture))]

// Define the namespace for test infrastructure
namespace xUnit.OTel.Tests;

// Test fixture class that sets up OpenTelemetry diagnostics for the entire test assembly
// This class implements IAsyncLifetime to properly manage setup and teardown of resources
// It configures a host with dependency injection, OpenTelemetry, and HTTP client instrumentation
public class TestFixture : IAsyncLifetime
{
    // Private field to store the application host that contains all configured services
    // The null-forgiving operator (!) indicates this will be initialized in InitializeAsync
    private IHost _host = null!;

    // Public property that exposes the configured host to test classes
    // This allows tests to access services like IHttpClientFactory through dependency injection
    public IHost Host => _host;

    // Async initialization method called once before any tests in the assembly run
    // This method configures the application host with all necessary services for testing
    public async ValueTask InitializeAsync()
    {

        // Create and configure the application host using the default builder pattern
        _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            // Configure the dependency injection container with required services
            .ConfigureServices(services =>
            {
                // Add OpenTelemetry diagnostics with default configuration
                // This sets up tracing, metrics, and logging for the test environment
                services.AddOTelDiagnostics();

                // Add HttpClient for testing HTTP instrumentation
                // This registers IHttpClientFactory and enables HTTP request tracing
                services.AddHttpClient();
            })
            // Build the configured host instance
            .Build();

        // Start the host to initialize all services and begin background services
        await _host.StartAsync();

        // Log the test run initialization using the configured logging system
        // This demonstrates that the logging integration is working correctly
        var logger = _host.Services.GetRequiredService<ILogger<TestFixture>>();
        logger.LogInformation("OpenTelemetry diagnostics configured with HTTP client instrumentation");
    }

    // Async disposal method called once after all tests in the assembly complete
    // This method ensures proper cleanup of resources and graceful shutdown
    public async ValueTask DisposeAsync()
    {
        // Log the disposal process to demonstrate logging works during teardown
        var logger = _host.Services.GetRequiredService<ILogger<TestFixture>>();
        logger.LogInformation("Test fixture disposing...");

        // Gracefully stop the host and all its background services
        await _host.StopAsync();
        
        // Dispose of the host to free all resources and complete cleanup
        _host.Dispose();
    }
}
