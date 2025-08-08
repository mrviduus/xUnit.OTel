// Import xUnit framework interfaces for test output functionality
using Xunit;

// Define the namespace for logging provider implementation
namespace xUnitOTel.Logging;

// Partial class continuation that contains constructors for XunitLoggerProvider
// This separation allows for cleaner organization of constructor logic
public partial class XunitLoggerProvider
{
    // Private field to store the test output helper accessor for dependency injection
    // This field is initialized through the constructor and used to create loggers
    private readonly ITestOutputHelperAccessor _outputHelperAccessor;


    // Constructor overload that accepts a specific ITestOutputHelper instance and options
    // This constructor creates a TestOutputHelperAccessor wrapper around the provided helper
    // It's useful when you have a direct reference to the test output helper
    public XunitLoggerProvider(ITestOutputHelper outputHelper, XunitLoggerOptions options)
        : this(new TestOutputHelperAccessor(outputHelper), options)
    {
        // Delegate to the main constructor using the accessor wrapper
        // This provides a convenient API while maintaining consistent internal structure
    }

    // Main constructor that accepts an output helper accessor and configuration options
    // This constructor performs validation and initializes the provider's core dependencies
    public XunitLoggerProvider(ITestOutputHelperAccessor outputHelperAccessor, XunitLoggerOptions options)
    {
        // Validate that the output helper accessor is not null to prevent runtime errors
        // Throw a descriptive exception if null to help with debugging configuration issues
        _outputHelperAccessor = outputHelperAccessor ?? throw new ArgumentNullException(nameof(outputHelperAccessor));
        
        // Validate that the options are not null to ensure proper logger configuration
        // Throw a descriptive exception if null to help identify configuration problems
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
}
