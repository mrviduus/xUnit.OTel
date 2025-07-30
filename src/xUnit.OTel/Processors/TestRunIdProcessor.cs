using System.Diagnostics;
using OpenTelemetry;

namespace xUnit.OTel.Processors;

public class TestRunIdProcessor: BaseProcessor<Activity>
{
    private static readonly Guid TestRunId = Guid.NewGuid();

    public override void OnStart(Activity data)
    {
        data.SetTag("testrun.id", TestRunId.ToString());
    }
}
