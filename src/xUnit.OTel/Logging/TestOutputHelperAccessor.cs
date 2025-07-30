using xUnit.OTel.Core;
using Xunit;

namespace xUnit.OTel.Logging;

public interface ITestOutputHelperAccessor
{
    ITestOutputHelper? Output { get; }
}

public class TestOutputHelperAccessor : ContextValue<ITestOutputHelper>, ITestOutputHelperAccessor
{
    public TestOutputHelperAccessor()
    {
    }

    public TestOutputHelperAccessor(ITestOutputHelper outputHelper)
    {
        Value = outputHelper;
    }

    public ITestOutputHelper? Output => Value ?? TestContext.Current?.TestOutputHelper;
}
