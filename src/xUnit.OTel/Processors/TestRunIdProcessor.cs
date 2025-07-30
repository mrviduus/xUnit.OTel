// Import System.Diagnostics for Activity class functionality
using System.Diagnostics;
// Import OpenTelemetry core functionality for processors
using OpenTelemetry;

// Define the namespace for OpenTelemetry processors
namespace xUnit.OTel.Processors;

// Processor class that adds a unique test run identifier to all activities
// This processor extends BaseProcessor<Activity> to automatically tag activities with test run information
// The test run ID helps correlate all activities from a single test execution session
public class TestRunIdProcessor: BaseProcessor<Activity>
{
    // Static readonly GUID that uniquely identifies this test run session
    // This ID is generated once when the class is first loaded and remains constant for the entire test run
    // It allows grouping and filtering activities by test run in telemetry backends
    private static readonly Guid TestRunId = Guid.NewGuid();

    // Override method that is called when an activity is started
    // This method automatically adds the test run ID as a tag to every activity
    public override void OnStart(Activity data)
    {
        // Add a tag with the test run ID to the activity
        // This tag will be included in all telemetry data and can be used for filtering and correlation
        // The "testrun.id" tag follows OpenTelemetry semantic conventions for test-related metadata
        data.SetTag("testrun.id", TestRunId.ToString());
    }
}
