using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace xUnit.OTel.Processors;

public class ActivityEventLogProcessor: BaseProcessor<LogRecord>
{
    private const string NoAttributesMessage = "No attributes were set for this log record.";
    public override void OnEnd(LogRecord? data)
    {
        if(data == null)
        {
            return;
        }
        base.OnEnd(data);

        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            var stateString = data.Attributes != null
                ? string.Join(", ", data.Attributes.Select(kv => $"{kv.Key}={kv.Value}"))
                : NoAttributesMessage;
            currentActivity.AddEvent(new ActivityEvent(stateString));
        }
    }
}
