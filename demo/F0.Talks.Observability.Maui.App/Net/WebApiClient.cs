namespace F0.Talks.Observability.Maui.App.Net;

public sealed class WebApiClient
{
	private readonly HttpClient _client;
	private readonly ILogger<WebApiClient> _logger;
	private readonly IMetrics _metrics;
	private readonly ITracer _tracer;

	public WebApiClient(HttpClient client, ILogger<WebApiClient> logger, IMetrics metrics, ITracer tracer)
	{
		_client = client;
		_logger = logger;
		_metrics = metrics;
		_tracer = tracer;

		_client.BaseAddress = new Uri(GetBaseAddress());
		_client.Timeout = TimeSpan.FromSeconds(10);

		static string GetBaseAddress()
		{
#if ANDROID
			Debug.Assert(DeviceInfo.Platform == DevicePlatform.Android);
			return "http://10.0.2.2:5144";
#else
			Debug.Assert(DeviceInfo.Platform != DevicePlatform.Android);
			return "http://localhost:5144";
#endif
		}
	}

	public async Task<string?> GetTodoItemAsync(int id)
	{
		using Activity? activity = _tracer.ActivitySource.StartActivity();

		_logger.FetchTodoItem(id);
		_metrics.TodoItem(id);

		Stream stream;
		using (_tracer.ActivitySource.StartActivity("webapi.fetch"))
		{
			stream = await _client.GetStreamAsync($"api/todos/{id}");
		}

		string? todoItem;
		using (_tracer.ActivitySource.StartActivity("webapi.parse"))
		{
			JsonDocument document = await JsonDocument.ParseAsync(stream);
			todoItem = document.RootElement.GetProperty("title").GetString();
		}

		return todoItem;
	}
}
