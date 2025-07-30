using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace xUnit.OTel.Logging;

[ProviderAlias("XUnit")]
public partial class XunitLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, XunitLogger> _loggers = [];
    private readonly XunitLoggerOptions _options;
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
    ~XunitLoggerProvider()
    {
        Dispose(false);
    }

    public virtual ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
            new XunitLogger(name, _outputHelperAccessor, _options, _scopeProvider));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        _loggers.Clear();
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider ?? new LoggerExternalScopeProvider();
    }

}
