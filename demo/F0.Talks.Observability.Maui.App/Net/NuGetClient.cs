namespace F0.Talks.Observability.Maui.App.Net;

public sealed class NuGetClient
{
	private static readonly ActivitySource s_source = new ActivitySource("MobileHeads.NuGet");

	private readonly HttpClient _client;
	private readonly ILogger<NuGetClient> _logger;
	private readonly IMetrics _metrics;

	public NuGetClient(HttpClient client, ILogger<NuGetClient> logger, IMetrics metrics)
	{
		_client = client;
		_logger = logger;
		_metrics = metrics;

		_client.BaseAddress = new Uri("https://azuresearch-usnc.nuget.org");
		_client.Timeout = TimeSpan.FromSeconds(10);
	}

	public async Task<long> GetTotalDownloadsAsync(string package = "Sentry.Maui")
	{
		using Activity? activity = s_source.StartActivity();

		_logger.DownloadNuGetPackage(package);
		_metrics.NuGetDownloads(package);

		Stream stream;
		using (s_source.StartActivity("nuget.fetch"))
		{
			stream = await _client.GetStreamAsync($"query?q=packageid:{package}&take=1&skip=0&prerelease=false");
		}

		long totalDownloads;
		using (s_source.StartActivity("nuget.parse"))
		{
			JsonDocument document = await JsonDocument.ParseAsync(stream);
			totalDownloads = document.RootElement.GetProperty("data").EnumerateArray().First().GetProperty("totalDownloads").GetInt64();
		}

		return totalDownloads;
	}
}
