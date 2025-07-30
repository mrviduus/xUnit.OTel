// Import System.Diagnostics for ActivitySource functionality
using System.Diagnostics;

// Define the namespace for xUnit OpenTelemetry diagnostics functionality
namespace xUnit.OTel.Diagnostics;

// Static class that provides centralized OpenTelemetry diagnostics configuration for the xUnit.OTel library
// This class serves as the main entry point for all activity sources and diagnostic operations
public static class ApplicationDiagnostics
{
    // Constant string that defines the name of the ActivitySource for this library
    // This name is used to identify activities created by xUnit.OTel in distributed tracing systems
    public const string ActivitySourceName = "xUnit.OTel.Diagnostics";

    // Static readonly ActivitySource instance that will be used throughout the library
    // This ActivitySource is responsible for creating Activity instances for distributed tracing
    // The ActivitySource is initialized once and reused for performance reasons
    public static readonly ActivitySource ActivitySource = new (ActivitySourceName);

}
