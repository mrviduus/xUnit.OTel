# xUnit.OTel

<p align="center">
  <img src="assets/images/icon.svg" alt="xUnit.OTel Logo" width="128" height="128">
</p>

[![Build Status](https://github.com/mrviduus/xUnit.OTel/workflows/Build%20and%20Test/badge.svg)](https://github.com/mrviduus/xUnit.OTel/actions)
[![NuGet Version](https://img.shields.io/nuget/v/xUnit.OTel.svg)](https://www.nuget.org/packages/xUnit.OTel/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/xUnit.OTel.svg)](https://www.nuget.org/packages/xUnit.OTel/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**🎯 What is this?** A tool that helps you see what your tests are doing, like a camera that records everything happening inside your tests!

**⚠️ Important:** This package only works with xUnit v3!

## 🌟 What Does It Do?

Imagine you're playing with toys and want to know:
- 🎬 When did you start playing?
- ⏱️ How long did you play?
- 🧸 Which toys did you use?
- 📍 What happened step by step?

This tool does the same for your code tests!

## 📦 Installation

Add it to your project:

```bash
dotnet add package xUnit.OTel
```

## 🚀 Super Simple Examples

### Example 1: Trace ALL Tests in Your Project (Easiest!)

Want to track every single test without adding `[Trace]` to each one? Here's the magic:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xUnit.OTel.Diagnostics;
using xUnit.OTel.Attributes;
using Xunit;

// 🎯 This ONE line tracks EVERY test in your project!
[assembly: Trace]

// You still need the setup
[assembly: AssemblyFixture(typeof(TestSetup))]

public class TestSetup : IAsyncLifetime
{
    public IHost Host { get; private set; }

    public async ValueTask InitializeAsync()
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Required for tracing to work
                services.AddOTelDiagnostics();
            })
            .Build();
        
        await Host.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Host.StopAsync();
        Host.Dispose();
    }
}

// Now ALL your tests are automatically tracked!
public class MyTests
{
    [Fact]
    public void Test1() // ✅ Automatically traced!
    {
        var result = 2 + 2;
        Assert.Equal(4, result);
    }

    [Fact]
    public void Test2() // ✅ Also automatically traced!
    {
        var name = "Hello World";
        Assert.Contains("Hello", name);
    }
}

public class MoreTests
{
    [Fact]
    public async Task WebTest() // ✅ This one too!
    {
        await Task.Delay(100);
        Assert.True(true);
    }
}
```

**🎉 Benefits:**
- No need to add `[Trace]` to every test
- All tests in all classes get tracked automatically
- Great for existing projects - just add one line!

### Example 2: Trace Individual Tests

If you want to trace only specific tests:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xUnit.OTel.Diagnostics;
using xUnit.OTel.Attributes;
using Xunit;

// First, set up the tracking system (required!)
[assembly: AssemblyFixture(typeof(TestSetup))]

public class TestSetup : IAsyncLifetime
{
    public IHost Host { get; private set; }

    public async ValueTask InitializeAsync()
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // This is required for [Trace] to work! 🎯
                services.AddOTelDiagnostics();
            })
            .Build();
        
        await Host.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Host.StopAsync();
        Host.Dispose();
    }
}

// Now your tests can use [Trace]
public class SimpleTests
{
    [Fact]
    [Trace] // 👈 Only this test gets traced
    public void MyFirstTest()
    {
        // Your test is now being tracked!
        var result = 2 + 2;
        Assert.Equal(4, result);
    }

    [Fact]
    public void MySecondTest() // This one is NOT traced
    {
        var result = 3 + 3;
        Assert.Equal(6, result);
    }
}
```

**📝 Note:** The `[Trace]` attribute won't work without calling `services.AddOTelDiagnostics()` first!

### Example 3: Testing Web Calls

Want to see what happens when your code talks to the internet?

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xUnit.OTel.Diagnostics;
using xUnit.OTel.Attributes;
using Xunit;

// Trace everything!
[assembly: Trace]
[assembly: AssemblyFixture(typeof(TestSetup))]

public class TestSetup : IAsyncLifetime
{
    public IHost Host { get; private set; }

    public async ValueTask InitializeAsync()
    {
        // Create a mini-application for testing
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Add the magic tracking ✨
                services.AddOTelDiagnostics();
                // Add ability to make web calls
                services.AddHttpClient();
            })
            .Build();
        
        await Host.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up when done
        await Host.StopAsync();
        Host.Dispose();
    }
}

// Your actual tests
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
        var response = await _httpClient.GetAsync("https://www.google.com");
        
        Assert.True(response.IsSuccessStatusCode);
    }
}
```

## 🏠 Where Do The Traces Go?

### Option 1: Console Output (Easy for Development)

Your traces appear right in the test output:

```csharp
services.AddOTelDiagnostics(
    configureTracerProviderBuilder: tracing => 
    {
        tracing.AddConsoleExporter(); // 👈 Shows traces in console
    }
);
```

### Option 2: OpenTelemetry Collector (For Real Projects)

Think of the OpenTelemetry Collector as a post office that collects all your test information and sends it where you want:

**Step 1: Run the Collector** (like starting the post office)

```bash
# Using Docker (easiest way)
docker run -p 4317:4317 \
  -v $(pwd)/otel-config.yaml:/etc/otel-collector-config.yaml \
  otel/opentelemetry-collector:latest \
  --config=/etc/otel-collector-config.yaml
```

**Step 2: Create a Simple Config** (`otel-config.yaml`)

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317

exporters:
  logging:
    loglevel: debug

service:
  pipelines:
    traces:
      receivers: [otlp]
      exporters: [logging]
```

**Step 3: Tell Your Tests Where to Send Data**

```csharp
services.AddOTelDiagnostics(
    configureTracerProviderBuilder: tracing => 
    {
        tracing.AddOtlpExporter(options =>
        {
            // Tell it where the collector is listening
            options.Endpoint = new Uri("http://localhost:4317");
        });
    }
);
```

## 📊 What Can You See?

When you run a test with tracing enabled, you'll see:

```
🧪 Test: MyFirstTest
├── ⏱️ Started: 10:30:15.123
├── ⏱️ Duration: 245ms
├── ✅ Status: Passed
├── 📍 Class: SimpleTests
└── 📊 Details:
    ├── 🌐 HTTP GET https://www.google.com (125ms)
    ├── 💾 Database Query (50ms)
    └── 🔄 Processing Data (70ms)
```

## 🎨 Real-World Example: Testing a Weather Service

```csharp
public class WeatherTests
{
    private readonly HttpClient _httpClient;

    public WeatherTests(TestSetup setup)
    {
        var factory = setup.Host.Services.GetRequiredService<IHttpClientFactory>();
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task CheckTodaysWeather()
    {
        // Call 1: Get location
        var locationResponse = await _httpClient.GetAsync("https://ipapi.co/json/");
        var location = await locationResponse.Content.ReadAsStringAsync();
        
        // Call 2: Get weather for that location
        var weatherResponse = await _httpClient.GetAsync($"https://wttr.in/London?format=j1");
        
        // The trace will show:
        // - Both HTTP calls
        // - How long each took
        // - What data was sent/received
        // - If anything failed
        
        Assert.True(weatherResponse.IsSuccessStatusCode);
    }
}
```

## 🤔 Common Questions

### "Why do I need a Host?"
Think of the Host as a mini-application that runs during your tests. It's like having a toy kitchen when you want to play cooking - you need the kitchen (Host) to use the stove (HTTP client) and other tools!

The Host is also where the OpenTelemetry system lives - it's like the control room that watches and records everything.

### "What's OpenTelemetry?"
It's like a security camera system for your code. It watches what happens and tells you about it!

### "Do I always need the Collector?"
No! For simple testing, use `AddConsoleExporter()` to see traces right in your test output. The Collector is for when you want to send traces to special monitoring tools like Jaeger or Zipkin.

### "Why doesn't [Trace] work by itself?"
The `[Trace]` attribute is like a light switch - but first you need to install the electrical system (`AddOTelDiagnostics()`)! Without the setup, the switch has nothing to connect to.

### "Should I use [assembly: Trace] or individual [Trace] attributes?"
- Use `[assembly: Trace]` when you want to trace everything (recommended for most projects)
- Use individual `[Trace]` attributes when you only want to trace specific tests

## 📋 Requirements

- .NET 8.0 or later
- xUnit v3 (won't work with xUnit v2!)

## 🆘 Need Help?

- 🐛 [Report Problems](https://github.com/mrviduus/xUnit.OTel/issues)
- 💬 [Ask Questions](https://github.com/mrviduus/xUnit.OTel/discussions)
- 📧 [Email Us](mailto:mrviduus@gmail.com)

## 🎉 Quick Wins

1. **See test duration**: Know which tests are slow
2. **Track HTTP calls**: See all web requests your test makes
3. **Find failures**: Quickly see what went wrong
4. **Understand flow**: See the order of operations

## 📚 More Examples

Check out our [examples folder](https://github.com/mrviduus/xUnit.OTel/tree/main/examples) for:
- Testing with databases
- Testing microservices
- Complex scenarios
- CI/CD integration

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.