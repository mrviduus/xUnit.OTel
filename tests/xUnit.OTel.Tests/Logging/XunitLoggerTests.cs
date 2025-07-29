using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using xUnit.OTel.Logging;

namespace xUnit.OTel.Tests.Logging;

public class FakeTestOutputHelper : ITestOutputHelper
{
    public List<string> Lines { get; } = new();

    public void WriteLine(string message)
    {
        Lines.Add(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        Lines.Add(string.Format(format, args));
    }
}

public class FakeAccessor : ITestOutputHelperAccessor
{
    public FakeAccessor(ITestOutputHelper helper) => Output = helper;
    public ITestOutputHelper? Output { get; }
}

public class XunitLoggerTests
{
    [Fact]
    public void Log_Writes_Message_To_Output()
    {
        var helper = new FakeTestOutputHelper();
        var accessor = new FakeAccessor(helper);
        var logger = new XunitLogger("Test", accessor, new XunitLoggerOptions());

        logger.LogInformation("Hello");

        Assert.Single(helper.Lines);
        Assert.Contains("Hello", helper.Lines[0]);
    }
}
