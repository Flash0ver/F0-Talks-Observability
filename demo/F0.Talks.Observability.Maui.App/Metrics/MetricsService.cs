using System.Diagnostics;
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
		TagList attributes = [];

		if (instrument.Meter.Tags is not null)
		{
			foreach (KeyValuePair<string, object?> tag in instrument.Meter.Tags)
			{
				if (tag.Value is not null)
				{
					attributes.Add(tag);
				}
			}
		}

		if (instrument.Tags is not null)
		{
			foreach (KeyValuePair<string, object?> tag in instrument.Tags)
			{
				if (tag.Value is not null)
				{
					attributes.Add(tag);
				}
			}
		}

		if (!tags.IsEmpty)
		{
			foreach (KeyValuePair<string, object?> tag in tags)
			{
				if (tag.Value is not null)
				{
					attributes.Add(tag);
				}
			}
		}

		if (instrument is Counter<T> or ObservableCounter<T>)
		{
			SentrySdk.Experimental.Metrics.EmitCounter(instrument.Name, measurement, attributes);
		}
		else if (instrument is Gauge<T> or ObservableGauge<T>)
		{
			MeasurementUnit unit = instrument.Unit is not null ? MeasurementUnit.Custom(instrument.Unit) : MeasurementUnit.None;
			SentrySdk.Experimental.Metrics.EmitGauge(instrument.Name, measurement, unit, attributes);
		}
		else if (instrument is Histogram<T>)
		{
			MeasurementUnit unit = instrument.Unit is not null ? MeasurementUnit.Custom(instrument.Unit) : MeasurementUnit.None;
			SentrySdk.Experimental.Metrics.EmitDistribution(instrument.Name, measurement, unit, attributes);
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
