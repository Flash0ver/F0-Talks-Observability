namespace F0.Talks.Observability.Web.Api.Logging;

internal static partial class Log
{
	[LoggerMessage(1_000, LogLevel.Error, "Instrument type {Instrument} not supported (Name: {Name}, Meter: {Meter})")]
	internal static partial void InstrumentTypeNotSupported(this ILogger logger, Type instrument, string name, string meter);

	[LoggerMessage(1_001, LogLevel.Error, "Instrument unit {Unit} not supported (Instrument: {Instrument}, Meter: {Meter})")]
	internal static partial void InstrumentUnitNotSupported(this ILogger logger, string unit, string instrument, string meter);

	[LoggerMessage(1_002, LogLevel.Error, "Measurement type {Measurement} not supported (Instrument: {Instrument}, Meter: {Meter})")]
	internal static partial void MeasurementTypeNotSupported(this ILogger logger, Type measurement, string instrument, string meter);

	[LoggerMessage(1_003, LogLevel.Warning, "Exporting observable instruments exceeded time allotted (Timeout: {Timeout}, Elapsed: {Elapsed})")]
	internal static partial void ExportObservableInstrumentsTimeout(this ILogger logger, TimeSpan timeout, TimeSpan elapsed);

	[LoggerMessage(2_000, LogLevel.Information, "{Method} Request: {Route}")]
	internal static partial void Request(this ILogger logger, string method, string route);

	extension(ILogger logger)
	{
		internal void InstrumentTypeNotSupported(Instrument instrument)
			=> logger.InstrumentTypeNotSupported(instrument.GetType(), instrument.Name, instrument.Meter.Name);

		internal void InstrumentUnitNotSupported(string unit, Instrument instrument)
			=> logger.InstrumentUnitNotSupported(unit, instrument.Name, instrument.Meter.Name);

		internal void MeasurementTypeNotSupported<T>(Instrument instrument) where T : struct
			=> logger.MeasurementTypeNotSupported(typeof(T), instrument.Name, instrument.Meter.Name);

		internal void Request(HttpRequest request)
			=> logger.Request(request.Method, request.Path);
	}
}
