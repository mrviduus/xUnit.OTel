using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using xUnit.OTel.Diagnostics;
using Xunit;

[assembly: AssemblyFixture(typeof(xUnit.OTel.Tests.TestFixture))]

namespace xUnit.OTel.Tests;

public class TestFixture : IAsyncLifetime
{
    private IHost _host = null!;

    public IHost Host => _host;

    public async ValueTask InitializeAsync()
    {

        _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddOTelDiagnostics();

                // Add HttpClient for testing HTTP instrumentation
                services.AddHttpClient();
            })
            .Build();

        await _host.StartAsync();

        // Log the test run initialization
        var logger = _host.Services.GetRequiredService<ILogger<TestFixture>>();
        logger.LogInformation("OpenTelemetry diagnostics configured with HTTP client instrumentation");
    }

    public async ValueTask DisposeAsync()
    {
        var logger = _host.Services.GetRequiredService<ILogger<TestFixture>>();
        logger.LogInformation("Test fixture disposing...");

        await _host.StopAsync();
        _host.Dispose();
    }
}
