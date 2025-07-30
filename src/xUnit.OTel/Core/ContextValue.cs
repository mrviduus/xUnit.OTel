// Import XUnit framework for test context functionality
using Xunit;
// Import XUnit v3 framework for enhanced test context capabilities
using Xunit.v3;

// Define the namespace for core xUnit OpenTelemetry functionality
namespace xUnit.OTel.Core;

// Generic class for managing context-aware values using AsyncLocal storage
// This class provides a thread-safe way to store and retrieve values that are scoped to the current async context
public class ContextValue<T>
{
    // AsyncLocal field to store the value holder in the current async context
    // This ensures that each async operation has its own isolated value
    private readonly AsyncLocal<ValueHolder> _contextValue = new();

    // Virtual property to get or set the context value
    // The getter returns the current value or default if no value is set
    // The setter manages the lifecycle of values in the async context
    public virtual T? Value
    {
        get
        {
            // Retrieve the value holder from the current async context
            var value = _contextValue.Value;
            // Return the stored value if it exists, otherwise return the default value for type T
            return value == null ? default : value.Value;
        }
        set
        {
            // Get the current value holder from the async context
            var holder = _contextValue.Value;
            // If a holder already exists, clear its value to prevent memory leaks
            if (holder != null)
            {
                // Clear current value trapped in the AsyncLocal, as its done.
                holder.Value = default;
            }

            // If the new value is not null, create a new holder and store it
            if (value != null)
            {
                // Set the new value in the AsyncLocal.
                _contextValue.Value = new ValueHolder { Value = value };
            }
        }
    }
    
    // Private nested class to hold the actual value
    // This indirection allows us to distinguish between null values and unset values
    private class ValueHolder
    {
        // Property to store the actual value of type T
        public T? Value { get; set; }
    }
}


