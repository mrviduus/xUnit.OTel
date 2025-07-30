using System.Diagnostics;
using System.Reflection;
using xUnit.OTel.Diagnostics;
using Xunit.Internal;
using Xunit.v3;

namespace xUnit.OTel.Attributes;
public class TraceAttribute : BeforeAfterTestAttribute
{
    private const string TestClassMethodTag = "test.class.method";
    private const string TestNameTag = "test.name";
    private const string TestFrameworkTag = "test.framework";
    private const string TestFrameworkName = "xunit";

    private static readonly Lazy<ConsoleCaptureTestOutputWriter> ConsoleCaptureWriter = new(() =>
        new ConsoleCaptureTestOutputWriter(TestContextAccessor.Instance, captureError: true, captureOut: true),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private Activity? _activity;

    public bool CaptureError { get; set; } = true;
    public bool CaptureOut { get; set; } = true;

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        // Initialize console capturing lazily and safely
        _ = ConsoleCaptureWriter.Value;

        var activityName = methodUnderTest.DeclaringType != null
            ? $"{methodUnderTest.DeclaringType.FullName}.{methodUnderTest.Name}"
            : methodUnderTest.Name;

        _activity = ApplicationDiagnostics.ActivitySource.StartActivity(activityName, ActivityKind.Internal);

        if (_activity is not null)
        {
            _activity.SetTag(TestClassMethodTag, activityName);
            _activity.SetTag(TestNameTag, methodUnderTest.Name);
            _activity.SetTag(TestFrameworkTag, TestFrameworkName);
        }
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        _activity?.Stop();
        if (_activity != null)
        {
            var traceId = _activity.TraceId.ToString();

            TestContextAccessor.Instance.Current?.TestOutputHelper?.WriteLine($"Trace ID: {traceId}");


            _activity.Dispose();
        }
    }
}
