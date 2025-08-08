// Import System.Diagnostics for Activity and ActivityKind classes
using System.Diagnostics;
// Import System.Reflection for MethodInfo class
using System.Reflection;
// Import custom diagnostics functionality for OpenTelemetry integration
using xUnitOTel.Diagnostics;
// Import xUnit internal functionality for console capture
using Xunit.Internal;
// Import xUnit v3 framework interfaces and classes
using Xunit.v3;

// Define the namespace for xUnit OpenTelemetry attributes
namespace xUnitOTel.Attributes;
// Define a custom attribute that extends BeforeAfterTestAttribute to add tracing capabilities
public class TraceAttribute : BeforeAfterTestAttribute
{
    // Constant string for the OpenTelemetry tag that identifies the test class and method
    private const string TestClassMethodTag = "test.class.method";
    // Constant string for the OpenTelemetry tag that identifies the test name
    private const string TestNameTag = "test.name";
    // Constant string for the OpenTelemetry tag that identifies the testing framework
    private const string TestFrameworkTag = "test.framework";
    // Constant string value identifying this as an xUnit test framework
    private const string TestFrameworkName = "xunit";

    // Lazy initialization of console capture writer to ensure thread-safe single instance creation
    // This captures both error and standard output streams for test execution
    private static readonly Lazy<ConsoleCaptureTestOutputWriter> ConsoleCaptureWriter = new(() =>
        // Create a new console capture writer that captures both error and standard output
        new ConsoleCaptureTestOutputWriter(TestContextAccessor.Instance, captureError: true, captureOut: true),
        // Ensure thread-safe lazy initialization with publication semantics
        LazyThreadSafetyMode.ExecutionAndPublication);

    // Private field to store the current OpenTelemetry Activity instance for this test execution
    private Activity? _activity;

    // Public property to control whether error output should be captured (defaults to true)
    public bool CaptureError { get; set; } = true;
    // Public property to control whether standard output should be captured (defaults to true)
    public bool CaptureOut { get; set; } = true;

    // Override method that executes before each test method runs
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        // Initialize console capturing lazily and safely by accessing the Value property
        // The underscore assignment discards the return value as we only need initialization
        _ = ConsoleCaptureWriter.Value;

        // Create a descriptive activity name using the full class name and method name
        // If the declaring type is null, fall back to just the method name
        var activityName = methodUnderTest.DeclaringType != null
            ? $"{methodUnderTest.DeclaringType.FullName}.{methodUnderTest.Name}"
            : methodUnderTest.Name;

        // Start a new OpenTelemetry Activity with the constructed name and Internal kind
        // This creates a new span in the distributed trace for this test execution
        _activity = ApplicationDiagnostics.ActivitySource.StartActivity(activityName, ActivityKind.Internal);

        // Check if the activity was successfully created (could be null if no listeners)
        if (_activity is not null)
        {
            // Set OpenTelemetry tags on the activity to provide metadata about the test
            // Tag for the full test class and method identification
            _activity.SetTag(TestClassMethodTag, activityName);
            // Tag for just the test method name
            _activity.SetTag(TestNameTag, methodUnderTest.Name);
            // Tag to identify this trace as coming from the xUnit framework
            _activity.SetTag(TestFrameworkTag, TestFrameworkName);
        }
    }

    // Override method that executes after each test method completes
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        // Stop the activity if it exists, marking the end of the test execution span
        _activity?.Stop();
        // Check if we have a valid activity instance to work with
        if (_activity != null)
        {
            // Extract the trace ID from the activity and convert it to a string
            // This provides a unique identifier for the entire distributed trace
            var traceId = _activity.TraceId.ToString();

            // Output the trace ID to the test output for debugging and correlation purposes
            // This allows developers to correlate test results with distributed traces
            TestContextAccessor.Instance.Current?.TestOutputHelper?.WriteLine($"Trace ID: {traceId}");


            // Dispose of the activity to free up resources and complete the span
            _activity.Dispose();
        }
    }
}
