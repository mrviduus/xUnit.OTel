using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace xUnit.OTel.Logging;

public class XunitLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ITestOutputHelperAccessor _outputHelperAccessor;
    private readonly XunitLoggerOptions _options;
    private readonly IExternalScopeProvider _scopeProvider;

    public XunitLogger(
        string categoryName,
        ITestOutputHelperAccessor outputHelperAccessor,
        XunitLoggerOptions options,
        IExternalScopeProvider scopeProvider)
    {
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _outputHelperAccessor = outputHelperAccessor ?? throw new ArgumentNullException(nameof(outputHelperAccessor));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return _scopeProvider.Push(state)!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && _options.Filter(_categoryName, logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter is null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var outputHelper = _outputHelperAccessor.Output;
        if (outputHelper is null)
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
        {
            return;
        }

        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(_options.TimestampFormat))
        {
            var timestamp = _options.TimeProvider.GetLocalNow().ToString(_options.TimestampFormat, CultureInfo.InvariantCulture);
            builder.Append(timestamp).Append(' ');
        }

        builder.Append('[').Append(logLevel.ToString()).Append("] ");
        builder.Append(_categoryName).Append(": ").Append(message);

        if (exception != null)
        {
            builder.Append(' ').Append(exception);
        }

        if (_options.IncludeScopes)
        {
            _scopeProvider.ForEachScope((scope, state) =>
            {
                state.Append(" => ").Append(scope);
            }, builder);
        }

        outputHelper.WriteLine(builder.ToString());
    }
}
