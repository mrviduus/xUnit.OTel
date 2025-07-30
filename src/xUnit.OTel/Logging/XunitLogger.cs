// Import Microsoft logging framework interfaces and types
using Microsoft.Extensions.Logging;
// Import globalization support for culture-invariant formatting
using System.Globalization;
// Import StringBuilder for efficient string building
using System.Text;

// Define the namespace for logger implementation
namespace xUnit.OTel.Logging;

// Logger implementation that writes log entries to xUnit test output
// This class integrates with the .NET logging framework while directing output to xUnit's test output system
// It supports all standard logging features including scopes, filtering, and custom formatting
public class XunitLogger : ILogger
{
    // Private fields to store logger configuration and dependencies
    // These are set once during construction and used throughout the logger's lifetime
    
    // The category name for this logger instance (typically the class name that requested the logger)
    private readonly string _categoryName;
    // Accessor for getting the current test's output helper
    private readonly ITestOutputHelperAccessor _outputHelperAccessor;
    // Configuration options that control logger behavior
    private readonly XunitLoggerOptions _options;
    // Provider for managing logging scopes
    private readonly IExternalScopeProvider _scopeProvider;

    // Constructor that initializes the logger with all required dependencies
    // All parameters are validated to ensure the logger operates correctly
    public XunitLogger(
        // The category name for this logger (usually the requesting class name)
        string categoryName,
        // Accessor for the test output helper to write log messages
        ITestOutputHelperAccessor outputHelperAccessor,
        // Configuration options for filtering and formatting
        XunitLoggerOptions options,
        // Provider for scope management and correlation
        IExternalScopeProvider scopeProvider)
    {
        // Validate all constructor parameters to prevent runtime errors
        // Use descriptive parameter names in exceptions to aid debugging
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _outputHelperAccessor = outputHelperAccessor ?? throw new ArgumentNullException(nameof(outputHelperAccessor));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
    }

    // Implementation of ILogger.BeginScope that creates a new logging scope
    // Scopes provide additional context that is automatically included in log messages
    // This method delegates to the scope provider for consistent scope management
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        // Delegate scope creation to the external scope provider
        // This ensures consistent scope behavior across different logger implementations
        return _scopeProvider.Push(state);
    }

    // Implementation of ILogger.IsEnabled that determines if logging is enabled for a specific level
    // This method is called before expensive log message formatting to improve performance
    public bool IsEnabled(LogLevel logLevel)
    {
        // Check that the log level is valid (not None) and passes the configured filter
        // The filter function can implement custom logic based on category and level
        return logLevel != LogLevel.None && _options.Filter(_categoryName, logLevel);
    }

    // Implementation of ILogger.Log that writes formatted log messages to the test output
    // This is the core logging method that handles message formatting, filtering, and output
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Early exit if logging is not enabled for this level to avoid unnecessary processing
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Validate that the formatter function is provided (required for message formatting)
        if (formatter is null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        // Get the current test output helper - exit early if none is available
        // This can happen if logging occurs outside of a test context
        var outputHelper = _outputHelperAccessor.Output;
        if (outputHelper is null)
        {
            return;
        }

        // Format the log message using the provided formatter function
        // The formatter combines the state and exception into a readable string
        var message = formatter(state, exception);
        
        // Skip logging if the formatted message is empty and there's no exception
        // This prevents outputting blank lines for empty log entries
        if (string.IsNullOrEmpty(message) && exception == null)
        {
            return;
        }

        // Build the complete log entry with optional timestamp, level, category, and message
        var builder = new StringBuilder();

        // Add timestamp prefix if a timestamp format is configured
        if (!string.IsNullOrEmpty(_options.TimestampFormat))
        {
            // Get the current local time and format it according to the configured format
            // Use InvariantCulture to ensure consistent formatting across different locales
            var timestamp = _options.TimeProvider.GetLocalNow().ToString(_options.TimestampFormat, CultureInfo.InvariantCulture);
            builder.Append(timestamp).Append(' ');
        }

        // Add the log level in brackets (e.g., [Information], [Warning], [Error])
        builder.Append('[').Append(logLevel.ToString()).Append("] ");
        
        // Add the category name and the formatted message
        builder.Append(_categoryName).Append(": ").Append(message);

        // Append exception information if an exception was provided
        if (exception != null)
        {
            // Add a space and then the full exception details (type, message, stack trace)
            builder.Append(' ').Append(exception);
        }

        // Include scope information if configured to do so
        if (_options.IncludeScopes)
        {
            // Iterate through all active scopes and append their string representations
            // This provides additional context like correlation IDs, request IDs, etc.
            _scopeProvider.ForEachScope((scope, state) =>
            {
                // Append each scope with an arrow separator for readability
                state.Append(" => ").Append(scope);
            }, builder);
        }

        // Write the complete formatted log entry to the xUnit test output
        // This will appear in the test results and can be viewed in test runners
        outputHelper.WriteLine(builder.ToString());
    }
}
