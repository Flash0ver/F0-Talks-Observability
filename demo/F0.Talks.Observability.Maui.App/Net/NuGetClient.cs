namespace F0.Talks.Observability.Maui.App.Net;

public sealed class NuGetClient
{
	private readonly HttpClient _client;
	private readonly ILogger<NuGetClient> _logger;
	private readonly IMetrics _metrics;
	private readonly ITracer _tracer;

	public NuGetClient(HttpClient client, ILogger<NuGetClient> logger, IMetrics metrics, ITracer tracer)
	{
		_client = client;
		_logger = logger;
		_metrics = metrics;
		_tracer = tracer;

		_client.BaseAddress = new Uri("https://azuresearch-usnc.nuget.org");
		_client.Timeout = TimeSpan.FromSeconds(10);
	}

	public async Task<long> GetTotalDownloadsAsync(string package)
	{
		using Activity? activity = _tracer.ActivitySource.StartActivity();

		_logger.DownloadNuGetPackage(package);
		_metrics.NuGetDownloads(package);

		Stream stream;
		using (_tracer.ActivitySource.StartActivity("nuget.fetch"))
		{
			stream = await _client.GetStreamAsync($"query?q=packageid:{package}&take=1&skip=0&prerelease=false");
		}

		long totalDownloads;
		using (_tracer.ActivitySource.StartActivity("nuget.parse"))
		{
			JsonDocument document = await JsonDocument.ParseAsync(stream);
			totalDownloads = document.RootElement.GetProperty("data").EnumerateArray().First().GetProperty("totalDownloads").GetInt64();
		}

		return totalDownloads;
	}
}
