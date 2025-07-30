// Import Microsoft logging framework for LogLevel enumeration
using Microsoft.Extensions.Logging;
// Import code analysis attributes for string syntax validation
using System.Diagnostics.CodeAnalysis;
// Import xUnit SDK for message sink functionality
using Xunit.Sdk;

// Define the namespace for logging options
namespace xUnit.OTel.Logging;

// Configuration class that defines options for the XunitLogger behavior
// This class follows the options pattern and can be configured through dependency injection
public class XunitLoggerOptions
{
    // Default constructor that initializes options with sensible defaults
    public XunitLoggerOptions()
    {
    }

    // Function property that determines whether a log entry should be written based on category and log level
    // By default, all log entries are allowed (returns true for any category and log level combination)
    // Can be customized to implement filtering logic (e.g., only log warnings and errors)
    public Func<string?, LogLevel, bool> Filter { get; set; } = static (c, l) => true; //By default, log everything
    
    // Function property that creates message sink messages from log strings
    // This factory converts log messages into xUnit's internal message format for integration
    // By default, creates DiagnosticMessage instances which are standard xUnit diagnostic outputs
    public Func<string, IMessageSinkMessage> MessageSinkMessageFactory { get; set; } = static (m) => new
        Xunit.v3.DiagnosticMessage(m);

    // Boolean property that controls whether logging scopes should be included in the output
    // When true, scope information (like request IDs, correlation IDs) will be appended to log messages
    // This provides additional context but may make logs more verbose
    public bool IncludeScopes { get; set; }
    
    // String property that specifies the format for timestamps in log messages
    // Uses StringSyntax attribute to provide IntelliSense and validation for DateTime format strings
    // When null or empty, no timestamp will be included in the log output
    [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
    public string? TimestampFormat { get; set; }

    // TimeProvider property for generating timestamps in log messages
    // Defaults to System time provider but can be replaced with custom implementations for testing
    // This abstraction allows for deterministic time in unit tests
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;
}
