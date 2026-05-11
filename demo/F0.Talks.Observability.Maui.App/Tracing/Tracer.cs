namespace F0.Talks.Observability.Maui.App.Tracing;

public interface ITracer
{
	ActivitySource ActivitySource { get; }
}

internal sealed class AppTracer : ITracer, IDisposable
{
	private readonly ActivitySource _source;

	public AppTracer()
	{
		_source = new ActivitySource("F0.Talks.Observability", "1.0.0");
	}

	ActivitySource ITracer.ActivitySource => _source;

	public void Dispose()
	{
		_source.Dispose();
	}
}
