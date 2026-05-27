namespace F0.Talks.Observability.Web.Api.Metrics;

internal sealed class MetricsService : IHostedService, IDisposable, IAsyncDisposable
{
	private readonly MeterListener _listener;
	private readonly Timer _timer;
	private readonly ILogger<MetricsService> _logger;

	public MetricsService(ILogger<MetricsService> logger)
	{
		_listener = new MeterListener();
		_timer = new Timer(OnIntervalElapsed);
		_logger = logger;
	}

	Task IHostedService.StartAsync(CancellationToken cancellationToken)
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

		_ = _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10_000));
		return Task.CompletedTask;
	}

	private void OnIntervalElapsed(object? state)
	{
		const int timeout = 1_000;

		Debug.Assert(Object.ReferenceEquals(state, _timer));
		long timestamp = Stopwatch.GetTimestamp();

		_listener.RecordObservableInstruments();

		TimeSpan elapsed = Stopwatch.GetElapsedTime(timestamp);
		if (elapsed.TotalMilliseconds > timeout)
		{
			_logger.ExportObservableInstrumentsTimeout(TimeSpan.FromMilliseconds(timeout), elapsed);
		}
	}

	Task IHostedService.StopAsync(CancellationToken cancellationToken)
	{
		_ = _timer.Change(Timeout.Infinite, Timeout.Infinite);
		return Task.CompletedTask;
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
			SentrySdk.Metrics.EmitGauge(instrument.Name, measurement, MeasurementUnit.From(instrument, _logger), attributes);
		}
		else if (instrument is Histogram<T>)
		{
			SentrySdk.Metrics.EmitDistribution(instrument.Name, measurement, MeasurementUnit.From(instrument, _logger), attributes);
		}
		else
		{
			_logger.InstrumentTypeNotSupported(instrument);
		}
	}

	private void OnUnsupportedMeasurementRecorded<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state) where T : struct
	{
		_logger.MeasurementTypeNotSupported<T>(instrument);
	}

	void IDisposable.Dispose()
	{
		_timer.Dispose();
		_listener.Dispose();
	}

	async ValueTask IAsyncDisposable.DisposeAsync()
	{
		await _timer.DisposeAsync();
		_listener.Dispose();
	}
}

file static class SentryMeasurementUnitExtensions
{
	extension(MeasurementUnit)
	{
		/// <seealso href="https://ucum.org/ucum">The Unified Code for Units of Measure</seealso>
		/// <seealso href="https://learn.microsoft.com/dotnet/core/diagnostics/built-in-metrics">Built-in metrics in .NET</seealso>
		/// <seealso href="https://develop.sentry.dev/sdk/foundations/state-management/scopes/attributes/#units">Sentry Units</seealso>
		public static MeasurementUnit From(Instrument instrument, ILogger logger)
		{
			string? unit = instrument.Unit;
			return unit switch
			{
				"s" => MeasurementUnit.Duration.Second,
				"ms" => MeasurementUnit.Duration.Millisecond,
				"ns" => MeasurementUnit.Duration.Nanosecond,
				"By" => MeasurementUnit.Information.Byte,
				"" => default(MeasurementUnit),
				null => default(MeasurementUnit),
				['{', _, .., '}'] => default(MeasurementUnit),
				_ => Default(unit, instrument, logger),
			};

			static MeasurementUnit Default(string unit, Instrument instrument, ILogger logger)
			{
				logger.InstrumentUnitNotSupported(unit, instrument);
				return MeasurementUnit.None;
			}
		}
	}
}
