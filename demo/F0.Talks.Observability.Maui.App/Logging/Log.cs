namespace F0.Talks.Observability.Maui.App.Logging;

internal static partial class Log
{
	[LoggerMessage(1_000, LogLevel.Error, "Instrument type {Instrument} not supported")]
	internal static partial void InstrumentTypeNotSupported(this ILogger logger, Type instrument);

	[LoggerMessage(2_000, LogLevel.Information, "Clicked Button {Count} times")]
	internal static partial void ClickedButton(this ILogger logger, int count);

	[LoggerMessage(2_001, LogLevel.Information, "Downloading NuGet Package {Package}")]
	internal static partial void DownloadNuGetPackage(this ILogger logger, string package);

	[LoggerMessage(2_002, LogLevel.Information, "Fetching TODO item {ID}")]
	internal static partial void FetchTodoItem(this ILogger logger, int id);
}
