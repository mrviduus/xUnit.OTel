// Import the core context value functionality for async context management
using xUnitOTel.Core;
// Import xUnit framework interfaces for test output
using Xunit;

// Define the namespace for logging functionality
namespace xUnitOTel.Logging;

// Interface that provides access to xUnit's ITestOutputHelper in a context-aware manner
// This abstraction allows different implementations for accessing test output helpers
public interface ITestOutputHelperAccessor
{
    // Property that returns the current test output helper instance or null if not available
    ITestOutputHelper? Output { get; }
}

// Implementation of ITestOutputHelperAccessor that uses async context to manage test output helper instances
// This class extends ContextValue<ITestOutputHelper> to provide thread-safe access to the current test's output helper
// It supports both explicit injection and automatic discovery from the current test context
public class TestOutputHelperAccessor : ContextValue<ITestOutputHelper>, ITestOutputHelperAccessor
{
    // Default constructor that creates an accessor without an initial output helper
    // The output helper can be set later through the Value property or retrieved from the test context
    public TestOutputHelperAccessor()
    {
    }

    // Constructor that initializes the accessor with a specific test output helper instance
    // This is useful when the output helper is known at construction time
    public TestOutputHelperAccessor(ITestOutputHelper outputHelper)
    {
        // Set the provided output helper as the current value in the async context
        Value = outputHelper;
    }

    // Property that provides access to the current test output helper
    // It first tries to get the value from the async context, then falls back to the current test context
    // This dual approach ensures compatibility with different xUnit usage patterns
    public ITestOutputHelper? Output => Value ?? TestContext.Current?.TestOutputHelper;
}
