using Xunit;
using xUnit.OTel.Core;

namespace xUnit.OTel.Logging;

public interface ITestOutputHelperAccessor
{
    ITestOutputHelper? Output { get; }
}
public class TestOutputHelperAccessor: ContextValue<ITestOutputHelper>, ITestOutputHelperAccessor
{
    public ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
}
