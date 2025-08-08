// Import Microsoft dependency injection services for service registration
using Microsoft.Extensions.DependencyInjection;
// Import TryAdd extension methods for safe service registration that avoids duplicates
using Microsoft.Extensions.DependencyInjection.Extensions;
// Import Microsoft logging framework interfaces and builders
using Microsoft.Extensions.Logging;
// Import Microsoft options pattern for configuration
using Microsoft.Extensions.Options;
// Import the logging namespace to access logging-related classes
using xUnitOTel.Logging;

// Define the namespace for logging extensions
namespace xUnitOTel.Logging;

// Static class containing extension methods for configuring xUnit output logging
// These extensions provide a fluent API for integrating xUnit test output with the .NET logging framework
public static class LoggingBuilderExtensions
{
    // Extension method that adds xUnit output logging to the logging builder with default configuration
    // This method registers all necessary services and providers for xUnit logging integration
    public static ILoggingBuilder AddXUnitOutput(this ILoggingBuilder builder)
    {
        // Register TestOutputHelperAccessor as a singleton service for managing test output context
        // TryAddSingleton ensures we don't duplicate registrations if called multiple times
        builder.Services.TryAddSingleton<TestOutputHelperAccessor>();
        
        // Register the interface mapping to use TestOutputHelperAccessor for ITestOutputHelperAccessor
        // This allows dependency injection to resolve the interface to the concrete implementation
        builder.Services.TryAddSingleton<ITestOutputHelperAccessor>(provider => 
            provider.GetRequiredService<TestOutputHelperAccessor>());

        // Register the XunitLoggerProvider as a singleton ILoggerProvider
        // This provider creates XunitLogger instances that write to xUnit test output
        builder.Services.AddSingleton<ILoggerProvider>(provider => new XunitLoggerProvider(
            provider.GetRequiredService<TestOutputHelperAccessor>(),
            provider.GetRequiredService<IOptions<XunitLoggerOptions>>().Value));

        // Return the builder to enable method chaining
        return builder;
    }

    // Overloaded extension method that adds xUnit output logging with custom configuration options
    // This allows callers to customize the logging behavior through the options pattern
    public static ILoggingBuilder AddXUnitOutput(this ILoggingBuilder builder,
        // Action delegate for configuring XunitLoggerOptions (filters, formatting, etc.)
        Action<XunitLoggerOptions> configureOptions)
    {
        // Configure the XunitLoggerOptions using the provided configuration action
        // This registers the options with the dependency injection container
        builder.Services.Configure(configureOptions);

        // Call the parameterless overload to complete the registration
        // This avoids code duplication while allowing for configuration customization
        return builder.AddXUnitOutput();
    }
}
