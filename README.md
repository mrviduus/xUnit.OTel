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

**🚀 Key Features:**
- **📺 Built-in Test Output**: All telemetry logs appear directly in your test output by default - no extra setup needed!
- **🔍 HTTP Request Tracing**: See every web call your tests make with timing and response details
- **⚡ Zero Configuration**: Works out of the box with minimal setup
- **🎯 Flexible Tracing**: Trace all tests or just specific ones with attributes

## 📦 Installation

Add it to your project:

```bash
dotnet add package xUnit.OTel
```

## 📺 See Your Tests in Action

**The best part?** All telemetry data appears directly in your test output by default! No need for external dashboards or complex setup - everything you need is right there in your test results.

<p align="center">
  <img src="assets/images/output.png" alt="Test Output Example" width="800">
</p>

As you can see above, HTTP requests, timing information, and trace IDs are automatically logged to your test output, making debugging and understanding your tests incredibly easy!

## 🚀 Quick Start

### Step 1: Add Assembly Trace (Trace ALL tests)
```csharp
// Add this to any test file to trace ALL tests in your project
[assembly: Trace]
[assembly: AssemblyFixture(typeof(TestSetup))]
```

### Step 2: Create TestSetup
See our complete working example in [tests/xUnit.OTel.Tests/TestSetup.cs](tests/xUnit.OTel.Tests/TestSetup.cs)

### Step 3: Write Tests
See our HTTP client testing example in [tests/xUnit.OTel.Tests/WebTests.cs](tests/xUnit.OTel.Tests/WebTests.cs)

**That's it!** Your tests will now show telemetry data in the output.
```

**🎉 Benefits:**
- No need to add `[Trace]` to every test
- All tests in all classes get tracked automatically
- Great for existing projects - just add one line!

### Alternative: Trace Individual Tests

If you want to trace only specific tests, use the `[Trace]` attribute on individual test methods instead of the assembly-level attribute.

**📝 Note:** The `[Trace]` attribute won't work without the `TestSetup` configuration!
```

## Viewing the Data of Tests

The `xUnit.OTel` package, when configured with OTLP exporters as shown in the example below, outputs span data via gRPC to `localhost:4317`. To visualize this telemetry data, you can use the **Aspire Dashboard** which provides a user-friendly interface for viewing traces, metrics, and logs.

### Using Aspire Dashboard with Docker

Run the Aspire Dashboard in a Docker container to view your test telemetry:

```bash
docker run -it -p 18888:18888 -p 4317:18889 -d -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true --name aspire-dashboard mcr.microsoft.com/dotnet/nightly/aspire-dashboard:8.0.0
```

Then navigate to **http://localhost:18888** to access the dashboard.



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