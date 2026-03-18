namespace F0.Talks.Observability.Maui.App;

public partial class MainPage : ContentPage
{
	private readonly NuGetClient _nuget;
	private readonly ILogger<MainPage> _logger;
	private readonly IMetrics _metrics;

	int count = 0;

	public MainPage(NuGetClient nuget, ILogger<MainPage> logger, IMetrics metrics)
	{
		_nuget = nuget;
		_logger = logger;
		_metrics = metrics;

		InitializeComponent();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);

		_logger.ClickedButton(count);
		_metrics.ButtonClicked();
	}

	private async void OnNuGetClicked(object? sender, EventArgs e)
	{
		NuGetBtn.IsEnabled = false;

		long totalDownloads = await _nuget.GetTotalDownloadsAsync("Sentry.Maui");
		NuGetBtn.Text = $"Total Downloads of Sentry.Maui: {totalDownloads:n0}";

		NuGetBtn.IsEnabled = true;
	}

	private void OnCrashClicked(object? sender, EventArgs e)
	{
		CrashBtn.Text = "Crashing..";
		throw new MobileHeadsException();
	}
}

file sealed class MobileHeadsException : Exception;
