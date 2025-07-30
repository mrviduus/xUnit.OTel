using Microsoft.Extensions.Hosting;
using xUnit.OTel.Diagnostics;
using Xunit;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(xUnit.OTel.Tests.TestFixture))]

namespace xUnit.OTel.Tests;

public class TestFixture : IAsyncLifetime
{
    private IHost _host = null!;

    public async ValueTask InitializeAsync()
    {
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var logPath = Path.Combine(
            solutionRoot,
            "logs",
            $"log-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.json");

        // Ensure the directory exists
        var logDirectory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddOTelDiagnostics();
            })
            .Build();

        await _host.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }
}
