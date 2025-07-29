# xUnit.OTel

[![Build Status](https://github.com/mrviduus/xUnit.OTel/workflows/Build%20and%20Test/badge.svg)](https://github.com/mrviduus/xUnit.OTel/actions)
[![NuGet](https://img.shields.io/nuget/v/xUnit.OTel.svg)](https://www.nuget.org/packages/xUnit.OTel/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

OpenTelemetry integration for xUnit testing framework, enabling distributed tracing and observability for your unit tests.

## Features

- 🔍 **Distributed Tracing**: Automatically trace your xUnit tests with OpenTelemetry
- 📊 **Test Observability**: Monitor test execution, performance, and behavior
- 🏷️ **Rich Metadata**: Add custom tags and metadata to your test traces
- 📝 **Enhanced Logging**: Integrate test output with OpenTelemetry logging
- 🔧 **Easy Integration**: Simple setup with minimal code changes
- 🚀 **Performance Monitoring**: Track test execution times and identify bottlenecks

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

```csharp
using xUnit.OTel;
using xUnit.OTel.Extensions;
using Xunit;
using Xunit.Abstractions;

public class MyTests
{
    private readonly XUnitTracer _tracer;
    private readonly ITestOutputHelper _output;

    public MyTests(ITestOutputHelper output)
    {
        _tracer = new XUnitTracer();
        _output = output.WithOpenTelemetry();
    }

    [Fact]
    public void MyTest()
    {
        using var activity = _tracer.StartTestMethod("MyTests", "MyTest");
        
        // Your test code here
        _output.WriteLine("Test is running");
        
        // Add custom metadata
        _tracer.AddTags(
            ("test.category", "unit"),
            ("test.priority", "high")
        );
        
        // Assert and mark as passed
        Assert.True(true);
        _tracer.MarkTestPassed();
    }

    public void Dispose()
    {
        _tracer?.Dispose();
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

### XUnitTracer
The main class for creating OpenTelemetry traces in your tests:

```csharp
var tracer = new XUnitTracer("MyApp.Tests");
using var activity = tracer.StartTest("TestName");
tracer.AddTags(("key", "value"));
tracer.MarkTestPassed();
```

### OTelTestOutputHelper
Enhanced test output helper with OpenTelemetry integration:

```csharp
public MyTests(ITestOutputHelper output)
{
    _output = output.WithOpenTelemetry();
}
```

### Extension Methods
Convenient extension methods for common scenarios:

```csharp
using var activity = _tracer.StartTestMethod("ClassName", "MethodName");
_tracer.MarkTestPassed();
_tracer.MarkTestSkipped("Reason");
```

## Advanced Usage

### Step-by-Step Tracing

```csharp
[Fact]
public void ComplexTest()
{
    using var testActivity = _tracer.StartTest("ComplexTest");
    
    using (var setupActivity = _tracer.StartStep("Setup"))
    {
        // Setup code
    }
    
    using (var executeActivity = _tracer.StartStep("Execute"))
    {
        // Test execution
    }
    
    using (var verifyActivity = _tracer.StartStep("Verify"))
    {
        // Verification
    }
    
    _tracer.MarkTestPassed();
}
```

### Error Handling

```csharp
[Fact]
public void TestWithErrorHandling()
{
    using var activity = _tracer.StartTest("TestWithErrorHandling");
    
    try
    {
        // Test code that might fail
    }
    catch (Exception ex)
    {
        _tracer.MarkAsFailed(ex);
        throw;
    }
}
```

## Requirements

- .NET 8.0 or later
- xUnit 2.4.2 or later
- OpenTelemetry 1.9.0 or later

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) and submit pull requests to the `develop` branch.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 🐛 [Report Issues](https://github.com/mrviduus/xUnit.OTel/issues)
- 💬 [Discussions](https://github.com/mrviduus/xUnit.OTel/discussions)
- 📧 [Contact](mailto:your.email@example.com)

## Acknowledgments

- [OpenTelemetry](https://opentelemetry.io/) for the observability framework
- [xUnit](https://xunit.net/) for the testing framework
- The .NET community for continuous support and feedback