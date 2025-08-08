// Import System.Diagnostics for Activity class functionality
using System.Diagnostics;
// Import OpenTelemetry core functionality for processors
using OpenTelemetry;
// Import OpenTelemetry logging functionality
using OpenTelemetry.Logs;

// Define the namespace for OpenTelemetry processors
namespace xUnitOTel.Processors;

// Processor class that converts log records into activity events for better trace correlation
// This processor extends BaseProcessor<LogRecord> to automatically attach log entries as events to the current activity
// This creates a unified view of logs and traces, making it easier to debug test issues
public class ActivityEventLogProcessor: BaseProcessor<LogRecord>
{
    // Constant message used when a log record has no attributes
    // This provides a fallback description for log entries without structured data
    private const string NoAttributesMessage = "No attributes were set for this log record.";
    
    // Override method that is called when a log record processing is completed
    // This method converts the log record into an activity event if there's an active activity
    public override void OnEnd(LogRecord? data)
    {
        // Check if the log record data is null and exit early if so
        // This prevents null reference exceptions and unnecessary processing
        if(data == null)
        {
            return;
        }
        
        // Call the base implementation to ensure proper processor lifecycle
        base.OnEnd(data);

        // Get the current activity from the ambient context
        // This will be null if no activity is currently active (e.g., outside of a traced operation)
        var currentActivity = Activity.Current;
        
        // Only process the log record if there's an active activity to attach it to
        if (currentActivity != null)
        {
            // Convert log record attributes to a string representation
            // If attributes exist, format them as key=value pairs; otherwise use a default message
            var stateString = data.Attributes != null
                ? string.Join(", ", data.Attributes.Select(kv => $"{kv.Key}={kv.Value}"))
                : NoAttributesMessage;
                
            // Add the log information as an event to the current activity
            // This creates a timeline of log entries within the trace span
            currentActivity.AddEvent(new ActivityEvent(stateString));
        }
    }
}
