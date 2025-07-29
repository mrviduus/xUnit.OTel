using System.Diagnostics;

namespace xUnit.OTel.Diagnostics;

public static class ApplicationDiagnostics
{
    public const string ActivitySourceName = "xUnit.OTel.Diagnostics";

    public static readonly ActivitySource ActivitySource = new (ActivitySourceName);

}
