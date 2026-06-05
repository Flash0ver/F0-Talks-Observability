namespace F0.Talks.Observability.Web.Api.Metrics;

internal static class AppMetrics
{
	internal static void Request(HttpRequest request)
	{
		Debug.Assert(request.Path.HasValue);
		Debug.Assert(!String.IsNullOrEmpty(request.Path.Value));

		SentrySdk.Metrics.EmitCounter("o11y.d3m0.http.requests", 1, [
			new KeyValuePair<string, object>("http.request.method", request.Method),
			new KeyValuePair<string, object>("http.request.route", request.Path.Value),
		]);
	}
}
