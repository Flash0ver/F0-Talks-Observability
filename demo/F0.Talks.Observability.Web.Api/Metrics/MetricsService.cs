namespace F0.Talks.Observability.Web.Api.Metrics;

internal sealed class MetricsService : BackgroundService
{
	private readonly MeterListener _listener;
	private readonly ILogger<MetricsService> _logger;

	public MetricsService(ILogger<MetricsService> logger)
	{
		_listener = new MeterListener();
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_listener.InstrumentPublished = static (Instrument instrument, MeterListener listener) =>
		{
			if (instrument.Meter.Name.Equals("System.Runtime", StringComparison.Ordinal) ||
				instrument.Meter.Name.StartsWith("System.Net.", StringComparison.Ordinal) ||
				instrument.Meter.Name.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal) ||
				instrument.Meter.Name.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal) ||
				instrument.Meter.Name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
			{
				listener.EnableMeasurementEvents(instrument);
			}
		};

		_listener.SetMeasurementEventCallback<byte>(OnMeasurementRecorded);
		_listener.SetMeasurementEventCallback<short>(OnMeasurementRecorded);
		_listener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
		_listener.SetMeasurementEventCallback<long>(OnMeasurementRecorded);
		_listener.SetMeasurementEventCallback<float>(OnMeasurementRecorded);
		_listener.SetMeasurementEventCallback<double>(OnMeasurementRecorded);
		_listener.SetMeasurementEventCallback<decimal>(OnUnsupportedMeasurementRecorded);

		_listener.Start();

		using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
		while (await timer.WaitForNextTickAsync(stoppingToken))
		{
			_listener.RecordObservableInstruments();
		}
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

		if (instrument is Counter<T> or ObservableCounter<T> or UpDownCounter<T> or ObservableUpDownCounter<T>)
		{
			SentrySdk.Metrics.EmitCounter(instrument.Name, measurement, attributes);
		}
		else if (instrument is Gauge<T> or ObservableGauge<T>)
		{
			SentrySdk.Metrics.EmitGauge(instrument.Name, measurement, MeasurementUnit.From(instrument.Unit, _logger), attributes);
		}
		else if (instrument is Histogram<T>)
		{
			SentrySdk.Metrics.EmitDistribution(instrument.Name, measurement, MeasurementUnit.From(instrument.Unit, _logger), attributes);
		}
		else
		{
			_logger.InstrumentTypeNotSupported(instrument.GetType());
		}
	}

	private void OnUnsupportedMeasurementRecorded<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state) where T : struct
	{
		_logger.MeasurementTypeNotSupported(instrument.GetType());
	}

	public override void Dispose()
	{
		base.Dispose();

		_listener.Dispose();
	}
}

file static class SentryMeasurementUnitExtensions
{
	extension(MeasurementUnit)
	{
		/// <seealso href="https://ucum.org/ucum"/>
		/// <seealso href="https://learn.microsoft.com/dotnet/core/diagnostics/built-in-metrics"/>
		public static MeasurementUnit From(string? unit, ILogger logger)
		{
			return unit switch
			{
				"s" => MeasurementUnit.Duration.Second,
				"By" => MeasurementUnit.Information.Byte,
				null => default(MeasurementUnit),
				_ => Default(unit, logger),
			};

			static MeasurementUnit Default(string unit, ILogger logger)
			{
				logger.InstrumentUnitNotSupported(unit);
				return MeasurementUnit.None;
			}
		}
	}
}
