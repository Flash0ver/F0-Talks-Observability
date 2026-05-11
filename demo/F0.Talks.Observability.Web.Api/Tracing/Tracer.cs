namespace F0.Talks.Observability.Web.Api.Tracing;

internal static class Tracer
{
	internal static async Task WaitAsync(CancellationToken cancellationToken = default)
	{
		ISpan span = SentrySdk.StartSpan("webapi.wait", "Wait");

		int milliseconds = Random.Shared.Next(10, 101);
		span.SetMeasurement("delay.ms", milliseconds, MeasurementUnit.Duration.Millisecond);

		try
		{
			TimeSpan delay = TimeSpan.FromMilliseconds(milliseconds);
			await Task.Delay(delay, cancellationToken);
		}
		catch (OperationCanceledException oce)
		{
			span.Finish(oce);
			throw;
		}
		catch (Exception e)
		{
			span.Finish(e);
			throw;
		}

		span.Finish();
	}
}
