using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using xUnit.OTel.Logging;
using xUnit.OTel.Processors;
using Xunit;
using Xunit.v3;

namespace xUnit.OTel.Diagnostics;

public static class OTelConfigurationExtensions
{
    public static IServiceCollection AddOTelDiagnostics(
        this IServiceCollection services,
        Action<ResourceBuilder>? configureResourceBuilder = null,
        Action<MeterProviderBuilder>? configureMeterProviderBuilder = null,
        Action<TracerProviderBuilder>? configureTracerProviderBuilder = null,
        Action<ILoggingBuilder>? configureLoggingBuilder = null)
    {
        services.TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();
        services.TryAddSingleton<Xunit.ITestContextAccessor>(_ => Xunit.v3.TestContextAccessor.Instance);

        services.AddOpenTelemetry().ConfigureOpenTelemetry(
            ApplicationDiagnostics.ActivitySourceName,
            configureResourceBuilder,
            configureMeterProviderBuilder,
            configureTracerProviderBuilder);

        services.ConfigureOpenTelemetryLogging(configureLoggingBuilder);

        return services;
    }

    private static OpenTelemetryBuilder ConfigureOpenTelemetry(
        this OpenTelemetryBuilder builder,
        string sourceName,
        Action<ResourceBuilder>? configureResource,
        Action<MeterProviderBuilder>? configureMetrics,
        Action<TracerProviderBuilder>? configureTracing)
    {
        builder
            .ConfigureResource(resource =>
            {
                resource.AddService(ApplicationDiagnostics.ActivitySourceName);
                configureResource?.Invoke(resource);
            })
            .WithMetrics(metrics =>
            {
                metrics.ConfigureDefaultMetrics(sourceName);
                configureMetrics?.Invoke(metrics);
            })
            .WithTracing(tracing =>
            {
                tracing.ConfigureDefaultTracing(sourceName);
                configureTracing?.Invoke(tracing);
            });

        return builder;
    }

    private static void ConfigureDefaultMetrics(
        this MeterProviderBuilder metrics,
        string sourceName)
    {
        metrics
            .AddMeter(sourceName)
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation();

#if DEBUG
        metrics.AddOtlpExporter();
#endif

    }

    private static void ConfigureDefaultTracing(
        this TracerProviderBuilder tracing,
        string sourceName)
    {
        tracing
            .AddHttpClientInstrumentation(o => o.RecordException = true)
            .AddSqlClientInstrumentation()
            .AddGrpcClientInstrumentation()
            .SetSampler(new AlwaysOnSampler())
            .AddSource(sourceName)
            .AddProcessor(new TestRunIdProcessor());

#if DEBUG
        tracing.AddOtlpExporter();
#endif

    }

    private static IServiceCollection ConfigureOpenTelemetryLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configureLogging)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();

            logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.AddProcessor(new ActivityEventLogProcessor());
#if DEBUG
                options.AddOtlpExporter();
#endif
            });

            logging.AddDebug();
            logging.AddConsole();
            logging.AddXUnitOutput();
        });
        return services;
    }
}
