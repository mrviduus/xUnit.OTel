using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace xUnit.OTel.Logging;

[ProviderAlias("XUnit")]
public partial class XunitLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, XunitLogger> _loggers = [];
    private readonly XunitLoggerOptions _options;
    ~XunitLoggerProvider()
    {
        Dispose(false);
    }

    public virtual ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, (name) =>
            _outputHelperAccessor is not null
                ? new XunitLogger(name, _outputHelperAccessor, _options)
                : new XunitLogger(name, _outputHelperAccessor, _options));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {

    }

}
