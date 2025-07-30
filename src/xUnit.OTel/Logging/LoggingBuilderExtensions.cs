using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using xUnit.OTel.Logging;

namespace xUnit.OTel.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddXUnitOutput(this ILoggingBuilder builder)
    {
        builder.Services.TryAddSingleton<TestOutputHelperAccessor>();
        builder.Services.TryAddSingleton<ITestOutputHelperAccessor>(provider => 
            provider.GetRequiredService<TestOutputHelperAccessor>());

        builder.Services.AddSingleton<ILoggerProvider>(provider => new XunitLoggerProvider(
            provider.GetRequiredService<TestOutputHelperAccessor>(),
            provider.GetRequiredService<IOptions<XunitLoggerOptions>>().Value));

        return builder;
    }

    public static ILoggingBuilder AddXUnitOutput(this ILoggingBuilder builder,
        Action<XunitLoggerOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);

        return builder.AddXUnitOutput();
    }
}
