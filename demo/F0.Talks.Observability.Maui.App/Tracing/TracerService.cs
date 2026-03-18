using OpenTelemetry;
using OpenTelemetry.Trace;

using Sentry.OpenTelemetry;

namespace F0.Talks.Observability.Maui.App.Tracing;

internal sealed class TracerService : IMauiInitializeService, IDisposable
{
	private TracerProvider? _tracerProvider;

	public void Initialize(IServiceProvider services)
	{
		_tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("Microsoft.Maui", "MobileHeads.NuGet")
			.AddSentry()
			.Build();
	}

	void IDisposable.Dispose()
	{
		_tracerProvider?.Dispose();
	}
}
