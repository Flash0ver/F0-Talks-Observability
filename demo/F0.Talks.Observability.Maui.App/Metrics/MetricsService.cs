using System.Diagnostics.Metrics;

namespace F0.Talks.Observability.Maui.App.Metrics;

internal sealed class MetricsService : IMauiInitializeService, IDisposable
{
	private MeterListener _listener = null!;
	private ILogger<MetricsService> _logger = null!;

	public void Initialize(IServiceProvider services)
	{
		_listener = new MeterListener();
		_logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<MetricsService>();

		_listener.InstrumentPublished = static (Instrument instrument, MeterListener listener) =>
		{
			if (instrument.Meter.Name is "Microsoft.Maui")
			{
				listener.EnableMeasurementEvents(instrument);
			}

			if (instrument.Meter.Name is "MobileHeads")
			{
				listener.EnableMeasurementEvents(instrument);
			}
		};

		_listener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
		_listener.SetMeasurementEventCallback<long>(OnMeasurementRecorded);

		_listener.Start();
	}

	private void OnMeasurementRecorded<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state) where T : struct
	{
		IEnumerable<KeyValuePair<string, object?>> attributes = [];

		if (instrument.Meter.Tags is not null)
		{
			attributes = attributes.Concat(instrument.Meter.Tags);
		}

		if (instrument.Tags is not null)
		{
			attributes = attributes.Concat(instrument.Tags);
		}

		if (!tags.IsEmpty)
		{
			attributes = attributes.Concat(tags.ToArray());
		}

		attributes = attributes.Where(static (KeyValuePair<string, object?> attribute) => attribute.Value is not null);

		if (instrument is Counter<T> or ObservableCounter<T>)
		{
			SentrySdk.Experimental.Metrics.EmitCounter(instrument.Name, measurement, attributes!);
		}
		else if (instrument is Gauge<T> or ObservableGauge<T>)
		{
			SentrySdk.Experimental.Metrics.EmitGauge(instrument.Name, measurement, null, attributes!);
		}
		else if (instrument is Histogram<T>)
		{
			SentrySdk.Experimental.Metrics.EmitDistribution(instrument.Name, measurement, null, attributes!);
		}
		else
		{
			_logger.InstrumentTypeNotSupported(instrument.GetType());
		}
	}

	void IDisposable.Dispose()
	{
		_listener.Dispose();
	}
}
