using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using xUnit.OTel.Logging;

namespace xUnit.OTel.Extensions;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddXUnitOutput(
        this ILoggingBuilder builder,
        Action<XunitLoggerOptions>? configure = null)
    {
        builder.Services.TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider>(sp =>
        {
            var accessor = sp.GetRequiredService<ITestOutputHelperAccessor>();
            var options = new XunitLoggerOptions();
            configure?.Invoke(options);
            return new XunitLoggerProvider(accessor, options);
        }));

        return builder;
    }

}
