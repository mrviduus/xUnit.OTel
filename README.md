# xUnit.OTel

<p align="center">
  <img src="icon.svg" alt="xUnit.OTel Logo" width="128" height="128">
</p>

[![Build Status](https://github.com/mrviduus/xUnit.OTel/workflows/Build%20and%20Test/badge.svg)](https://github.com/mrviduus/xUnit.OTel/actions)
[![NuGet Version](https://img.shields.io/nuget/v/xUnit.OTel.svg)](https://www.nuget.org/packages/xUnit.OTel/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/xUnit.OTel.svg)](https://www.nuget.org/packages/xUnit.OTel/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

OpenTelemetry integration for xUnit v3 testing framework, providing automatic distributed tracing and observability for your unit tests with minimal setup.

## Features

- 🔍 **Automatic Test Tracing**: Use `[Trace]` attribute to automatically trace test methods
- 📊 **HTTP Instrumentation**: Automatic tracing of HTTP requests with detailed telemetry
- 🏷️ **Rich Metadata**: Automatically tags tests with class, method, and framework information
- 📝 **Integrated Logging**: OpenTelemetry logging integration with xUnit test output
- 🔧 **Dependency Injection**: Full integration with .NET dependency injection
- 🚀 **Multiple Exporters**: Support for OTLP, Console, and custom exporters
- ⚡ **Low Overhead**: Optimized for test scenarios with minimal performance impact
- 🎯 **xUnit v3 Only**: Built exclusively for xUnit v3 testing framework

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package xUnit.OTel
```

Or via Package Manager Console:

```powershell
Install-Package xUnit.OTel
```

## Quick Start

### Simple Attribute-Based Tracing

```csharp
using xUnit.OTel.Attributes;
using Xunit;

public class MyTests
{
    [Fact]
    [Trace] // Automatically traces this test method
    public void MyTest()
    {
        // Your test code here - automatically traced!
        Assert.True(true);
    }
}
```

### Full Integration with Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xUnit.OTel.Diagnostics;
using xUnit.OTel.Attributes;
using Xunit;

// Set up test fixture for the assembly
[assembly: AssemblyFixture(typeof(MyTestFixture))]

public class MyTestFixture : IAsyncLifetime
{
    private IHost _host = null!;
    public IHost Host => _host;

    public async ValueTask InitializeAsync()
    {
        _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Add OpenTelemetry diagnostics with default configuration
                services.AddOTelDiagnostics();
                // Add HttpClient for testing HTTP instrumentation
                services.AddHttpClient();
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

public class MyTests
{
    private readonly HttpClient _httpClient;

    public MyTests(MyTestFixture fixture)
    {
        var httpClientFactory = fixture.Host.Services.GetRequiredService<IHttpClientFactory>();
        _httpClient = httpClientFactory.CreateClient();
    }

    [Fact]
    [Trace] // Traces HTTP calls automatically
    public async Task HttpTest()
    {
        var response = await _httpClient.GetAsync("https://httpbin.org/get");
        response.EnsureSuccessStatusCode();
    }
}
```

## Documentation

- 📚 [Getting Started Guide](docs/getting-started.md)
- 📖 [API Reference](docs/api-reference.md)
- 💡 [Examples](docs/examples.md)
- ⚙️ [Configuration](docs/configuration.md)
- 🔧 [Troubleshooting](docs/troubleshooting.md)

## Key Components

### TraceAttribute
The main attribute for adding OpenTelemetry tracing to test methods:

```csharp
[Fact]
[Trace] // Automatically creates spans for this test
public void MyTest()
{
    // Test code is automatically traced
    // HTTP calls, database calls, etc. are captured
}
```

### OTelConfigurationExtensions
Service collection extensions for configuring OpenTelemetry:

```csharp
services.AddOTelDiagnostics(
    configureResourceBuilder: resource => resource.AddService("MyTestApp"),
    configureTracerProviderBuilder: tracing => tracing.AddConsoleExporter(),
    configureMeterProviderBuilder: metrics => metrics.AddConsoleExporter(),
    configureLoggingBuilder: logging => logging.AddConsole()
);
```

### TestFixture Integration
Assembly-level test fixture for shared OpenTelemetry configuration:

```csharp
[assembly: AssemblyFixture(typeof(TestFixture))]

public class TestFixture : IAsyncLifetime
{
    public IHost Host { get; private set; }
    
    public async ValueTask InitializeAsync()
    {
        Host = CreateHostWithOTel();
        await Host.StartAsync();
    }
}
```

## Advanced Usage

### Custom Configuration

```csharp
services.AddOTelDiagnostics(
    configureResourceBuilder: resource => 
    {
        resource.AddService("MyTestService", "1.0.0");
        resource.AddAttributes(new[] 
        {
            new KeyValuePair<string, object>("environment", "test")
        });
    },
    configureTracerProviderBuilder: tracing =>
    {
        tracing.AddJaegerExporter();
        tracing.SetSampler(new TraceIdRatioBasedSampler(0.5));
    }
);
```

### HTTP Client Instrumentation

```csharp
[Fact]
[Trace]
public async Task HttpClientTest()
{
    // HTTP calls are automatically instrumented
    var response = await _httpClient.GetAsync("https://api.example.com/data");
    
    // Spans will include:
    // - HTTP method, URL, status code
    // - Request/response headers
    // - Timing information
    // - Error details if request fails
}
```

### Multiple Concurrent Requests

```csharp
[Fact]
[Trace]
public async Task ConcurrentRequestsTest()
{
    var urls = new[]
    {
        "https://api.example.com/endpoint1",
        "https://api.example.com/endpoint2",
        "https://api.example.com/endpoint3"
    };

    var tasks = urls.Select(url => _httpClient.GetAsync(url));
    var responses = await Task.WhenAll(tasks);
    
    // Each HTTP call creates its own span
    // All spans are correlated under the test span
}
```

## Requirements

- .NET 8.0 or later
- **xUnit v3 framework only** (not compatible with xUnit v2)
- OpenTelemetry 1.9.0 or later

## Built-in Instrumentation

The library automatically includes instrumentation for:

- **HTTP Client**: All HTTP requests are traced with detailed metadata
- **SQL Client**: Database operations are captured (when SqlClient is used)
- **gRPC Client**: gRPC calls are automatically instrumented  
- **Process Metrics**: CPU, memory, and process-level metrics
- **Runtime Metrics**: .NET GC, thread pool, and JIT metrics
- **Custom Test Spans**: Each test method becomes a span with rich metadata

## Trace Correlation

Every test execution includes:
- **Trace ID**: Unique identifier for the entire test trace
- **Test Metadata**: Class name, method name, framework information
- **Console Output**: Test output is captured and correlated with traces
- **Child Spans**: HTTP calls, DB operations, etc. are child spans of the test

## Debug vs Release Builds

- **Debug builds**: Include OTLP exporters for development/debugging
- **Release builds**: Optimized configuration for CI/CD environments

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) and submit pull requests to the `develop` branch.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 🐛 [Report Issues](https://github.com/mrviduus/xUnit.OTel/issues)
- 💬 [Discussions](https://github.com/mrviduus/xUnit.OTel/discussions)
- 📧 [Contact](mailto:mrviduus@gmail.com)

## Acknowledgments

- [OpenTelemetry](https://opentelemetry.io/) for the observability framework
- [xUnit](https://xunit.net/) for the testing framework
- The .NET community for continuous support and feedback