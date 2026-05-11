namespace F0.Talks.Observability.Maui.App;

public partial class MainPage : ContentPage
{
	private readonly NuGetClient _nuget;
	private readonly WebApiClient _webapi;
	private readonly ILogger<MainPage> _logger;
	private readonly IMetrics _metrics;

	private int _count = 0;

	public MainPage(NuGetClient nuget, WebApiClient webapi, ILogger<MainPage> logger, IMetrics metrics)
	{
		_nuget = nuget;
		_webapi = webapi;
		_logger = logger;
		_metrics = metrics;

		InitializeComponent();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		_count++;

		if (_count == 1)
			CounterBtn.Text = $"Clicked {_count} time";
		else
			CounterBtn.Text = $"Clicked {_count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);

		_logger.ClickedButton(_count);
		_metrics.ButtonClicked();
	}

	private async void OnNuGetClicked(object? sender, EventArgs e)
	{
		NuGetBtn.IsEnabled = false;

		long totalDownloads = await _nuget.GetTotalDownloadsAsync("Sentry.Maui");
		NuGetBtn.Text = $"Total Downloads of Sentry.Maui: {totalDownloads:n0}";

		NuGetBtn.IsEnabled = true;
	}

	private async void OnWebApiClicked(object? sender, EventArgs e)
	{
		WebApiBtn.IsEnabled = false;

		int id = Random.Shared.Next(1, 6);
		string? todoItem = await _webapi.GetTodoItemAsync(id);
		WebApiBtn.Text = $"Title of TODO item {id}: {todoItem}";

		WebApiBtn.IsEnabled = true;
	}

	private void OnCrashClicked(object? sender, EventArgs e)
	{
		CrashBtn.Text = "Crashing..";

#if ANDROID || IOS
		throw new TapException();
#else
		throw new ClickException();
#endif
	}
}
