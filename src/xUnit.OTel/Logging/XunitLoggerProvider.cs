// Import concurrent collections for thread-safe logger storage
using System.Collections.Concurrent;
// Import Microsoft logging framework interfaces
using Microsoft.Extensions.Logging;

// Define the namespace for logging provider implementation
namespace xUnit.OTel.Logging;

// Partial class that implements ILoggerProvider for xUnit test output integration
// This provider creates and manages XunitLogger instances that write to xUnit test output
// The ProviderAlias attribute allows this provider to be referenced by name in configuration
[ProviderAlias("XUnit")]
public partial class XunitLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    // Thread-safe dictionary to store and reuse logger instances by category name
    // This prevents creating multiple loggers for the same category and ensures consistent behavior
    private readonly ConcurrentDictionary<string, XunitLogger> _loggers = [];
    
    // Configuration options for the logger behavior (filtering, formatting, etc.)
    private readonly XunitLoggerOptions _options;
    
    // External scope provider for managing logging scopes across the application
    // Initialized with a default implementation but can be replaced via SetScopeProvider
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
    
    // Finalizer that ensures proper cleanup when the provider is garbage collected
    // This is a safety net in case Dispose() is not called explicitly
    ~XunitLoggerProvider()
    {
        // Call Dispose with false to indicate this is being called from the finalizer
        Dispose(false);
    }

    // Implementation of ILoggerProvider.CreateLogger that creates or retrieves cached logger instances
    // This method is thread-safe and ensures only one logger exists per category name
    public virtual ILogger CreateLogger(string categoryName)
    {
        // Use GetOrAdd to atomically retrieve existing logger or create new one if it doesn't exist
        // This pattern ensures thread safety and prevents duplicate logger creation
        return _loggers.GetOrAdd(categoryName, name =>
            new XunitLogger(name, _outputHelperAccessor, _options, _scopeProvider));
    }

    // Implementation of IDisposable.Dispose that cleans up resources and suppresses finalization
    public void Dispose()
    {
        // Call the virtual Dispose method with true to indicate explicit disposal
        Dispose(true);
        // Suppress finalization since we've already cleaned up
        GC.SuppressFinalize(this);
    }
    
    // Virtual dispose method that allows derived classes to override cleanup behavior
    // The disposing parameter indicates whether this is being called from Dispose() (true) or finalizer (false)
    protected virtual void Dispose(bool disposing)
    {
        // Clear all cached loggers to free memory and resources
        // This also ensures no further logging can occur through these loggers
        _loggers.Clear();
    }

    // Implementation of ISupportExternalScope.SetScopeProvider for external scope management
    // This allows the logging framework to provide scope information to this provider
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        // Set the scope provider, falling back to default implementation if null is provided
        // This ensures the provider always has a valid scope provider instance
        _scopeProvider = scopeProvider ?? new LoggerExternalScopeProvider();
    }

}
