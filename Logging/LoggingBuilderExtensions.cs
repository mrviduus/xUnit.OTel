using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using xUnit.OTel.Logging;

namespace xUnit.OTel.Extensions;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddXUnitOutput(this ILoggingBuilder builder)
    {
        builder.Services.TryAddSingleton<ILoggerProvider>(provider => new XunitLoggerProvider(
            ));
        return builder;
    }

}
