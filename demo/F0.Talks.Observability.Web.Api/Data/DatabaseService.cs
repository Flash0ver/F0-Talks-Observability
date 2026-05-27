namespace F0.Talks.Observability.Web.Api.Data;

internal sealed class DatabaseService : IHostedService, IDisposable, IAsyncDisposable
{
	private readonly IServiceProvider _services;

	public DatabaseService(IServiceProvider services)
	{
		_services = services;
	}

	async Task IHostedService.StartAsync(CancellationToken cancellationToken)
	{
		var factory = _services.GetRequiredService<TaskListContextFactory>();
		var logger = _services.GetRequiredService<ILogger<DatabaseService>>();

		await using TaskListContext context = factory.CreateDbContext();

		if (await context.Database.EnsureCreatedAsync(cancellationToken))
		{
			logger.DatabaseCreated();
		}
	}

	async Task IHostedService.StopAsync(CancellationToken cancellationToken)
	{
		var factory = _services.GetRequiredService<TaskListContextFactory>();
		var logger = _services.GetRequiredService<ILogger<DatabaseService>>();

		await using TaskListContext context = factory.CreateDbContext();

		if (await context.Database.EnsureDeletedAsync(cancellationToken))
		{
			logger.DatabaseDeleted();
		}
	}

	void IDisposable.Dispose()
	{
	}

	ValueTask IAsyncDisposable.DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}
}
