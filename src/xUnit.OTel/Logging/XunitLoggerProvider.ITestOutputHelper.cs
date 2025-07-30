using Xunit;

namespace xUnit.OTel.Logging;

public partial class XunitLoggerProvider
{
    private readonly ITestOutputHelperAccessor _outputHelperAccessor;


    public XunitLoggerProvider(ITestOutputHelper outputHelper, XunitLoggerOptions options)
        : this(new TestOutputHelperAccessor(outputHelper), options)
    {
    }

    public XunitLoggerProvider(ITestOutputHelperAccessor outputHelperAccessor, XunitLoggerOptions options)
    {
        _outputHelperAccessor = outputHelperAccessor ?? throw new ArgumentNullException(nameof(outputHelperAccessor));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
}
