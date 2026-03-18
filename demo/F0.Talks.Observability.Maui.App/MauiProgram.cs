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
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSentry(static (SentryMauiOptions options) =>
			{
				options.Dsn = null;
				options.Debug = true;
				options.SampleRate = 1.0f;
				options.TracesSampleRate = 1.0d;
				options.EnableLogs = true;
				options.Experimental.EnableMetrics = true;

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

		builder.Services.AddTransient<IMauiInitializeService, TracerService>();

		builder.Services.AddHttpClient<NuGetClient>();

		builder.Services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
			.WithLogging(logging =>
			{
				logging.AddConsoleExporter();
			})
			.WithMetrics(metrics =>
			{
				metrics.AddConsoleExporter((exporterOptions, metricReaderOptions) =>
				{
					metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
				});
			})
			.WithTracing(tracing =>
			{
				tracing.AddConsoleExporter();
			});

		return builder.Build();
	}
}
