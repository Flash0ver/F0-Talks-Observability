using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sentry.Maui;
using Sentry.OpenTelemetry;

namespace F0.Talks.Observability.Maui.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		MauiAppBuilder builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSentry(static (SentryMauiOptions options) =>
			{
				options.Dsn = null;
				options.Debug = true;
				options.SampleRate = 1.0f;
				options.TracesSampleRate = 1.0d;
				options.EnableLogs = true;
				options.EnableMetrics = true;

				options.Native.ExperimentalOptions.SessionReplay.OnErrorSampleRate = 1.0;
				options.Native.ExperimentalOptions.SessionReplay.SessionSampleRate = 1.0;
				options.Native.ExperimentalOptions.SessionReplay.MaskAllImages = true;
				options.Native.ExperimentalOptions.SessionReplay.MaskAllText = true;

				options.UseOpenTelemetry();
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Logging.AddFilter(null, LogLevel.Information);
		builder.Logging.AddFilter("F0.Talks.Observability.Maui.App", LogLevel.Information);

		builder.Services.AddMetrics();
		builder.Services.AddSingleton<IMetrics, AppMetrics>();
		builder.Services.AddTransient<IMauiInitializeService, MetricsService>();

		builder.Services.AddSingleton<ITracer, AppTracer>();
		builder.Services.AddTransient<IMauiInitializeService, TracerService>();

		builder.Services.AddHttpClient<NuGetClient>()
			.AddHttpMessageHandler(static () => new SentryHttpMessageHandler());
		builder.Services.AddHttpClient<WebApiClient>()
			.AddHttpMessageHandler(static () => new SentryHttpMessageHandler());

		builder.Services.AddOpenTelemetry()
			.ConfigureResource((ResourceBuilder resource) => resource.AddService(builder.Environment.ApplicationName))
			.WithLogging(static (LoggerProviderBuilder logging) =>
			{
				logging.AddConsoleExporter();
			})
			.WithMetrics(static (MeterProviderBuilder metrics) =>
			{
				metrics.AddConsoleExporter(static (ConsoleExporterOptions exporterOptions, MetricReaderOptions metricReaderOptions) =>
				{
					metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1_000;
				});
			})
			.WithTracing(static (TracerProviderBuilder tracing) =>
			{
				tracing.AddConsoleExporter();
			});

		return builder.Build();
	}
}
