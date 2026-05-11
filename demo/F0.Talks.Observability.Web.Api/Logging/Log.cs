namespace F0.Talks.Observability.Web.Api.Logging;

internal static partial class Log
{
	[LoggerMessage(1_000, LogLevel.Error, "Instrument type {Instrument} not supported")]
	internal static partial void InstrumentTypeNotSupported(this ILogger logger, Type instrument);

	[LoggerMessage(1_001, LogLevel.Error, "Instrument unit {Unit} not supported")]
	internal static partial void InstrumentUnitNotSupported(this ILogger logger, string unit);

	[LoggerMessage(1_002, LogLevel.Error, "Measurement type {Measurement} not supported")]
	internal static partial void MeasurementTypeNotSupported(this ILogger logger, Type measurement);

	[LoggerMessage(2_000, LogLevel.Information, "{Method} Request: {Route}")]
	internal static partial void Request(this ILogger logger, string method, string route);

	internal static void Request(this ILogger logger, HttpRequest request) => logger.Request(request.Method, request.Path);
}
