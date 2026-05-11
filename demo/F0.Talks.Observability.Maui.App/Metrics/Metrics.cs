namespace F0.Talks.Observability.Maui.App.Metrics;

public interface IMetrics
{
	void ButtonClicked();
	void NuGetDownloads(string package);
	void TodoItem(int id);
}

internal sealed class AppMetrics : IMetrics, IDisposable
{
	private readonly IMeterFactory _meters;
	private readonly Meter _meter;

	private readonly Counter<long> _buttonClicked;
	private readonly Counter<long> _nuGetDownloads;
	private readonly Counter<long> _todoItem;

	public AppMetrics(IMeterFactory meters)
	{
		_meters = meters;

		_meter = _meters.Create("F0.Talks.Observability", "1.0.0", [
			KeyValuePair.Create<string, object?>("device.model", DeviceInfo.Current.Model),
			KeyValuePair.Create<string, object?>("device.manufacturer", DeviceInfo.Current.Manufacturer),
			KeyValuePair.Create<string, object?>("device.name", DeviceInfo.Current.Name),
			KeyValuePair.Create<string, object?>("device.version", DeviceInfo.Current.VersionString),
			KeyValuePair.Create<string, object?>("device.platform", DeviceInfo.Current.Platform),
			KeyValuePair.Create<string, object?>("device.idiom", DeviceInfo.Current.Idiom),
			KeyValuePair.Create<string, object?>("device.type", DeviceInfo.Current.DeviceType),
		]);

		_buttonClicked = _meter.CreateCounter<long>("o11y.d3m0.button.clicked");
		_nuGetDownloads = _meter.CreateCounter<long>("o11y.d3m0.nuget.downloads");
		_todoItem = _meter.CreateCounter<long>("o11y.d3m0.todo.item");
	}

	void IMetrics.ButtonClicked() => _buttonClicked.Add(1);
	void IMetrics.NuGetDownloads(string package) => _nuGetDownloads.Add(1, [KeyValuePair.Create<string, object?>("package.id", package)]);
	void IMetrics.TodoItem(int id) => _todoItem.Add(1, [KeyValuePair.Create<string, object?>("todo.id", id)]);

	public void Dispose()
	{
		_meters.Dispose();
		_meter.Dispose();
	}
}
