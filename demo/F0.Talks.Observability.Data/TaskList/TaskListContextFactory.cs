using Microsoft.Data.Sqlite;

namespace F0.Talks.Observability.Data.TaskList;

public sealed class TaskListContextFactory : IDisposable, IAsyncDisposable
{
	private readonly SqliteConnection _connection;
	private readonly DbContextOptions<TaskListContext> _options;

	public TaskListContextFactory()
	{
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		_options = new DbContextOptionsBuilder<TaskListContext>()
			.UseSqlite(_connection)
			.UseSeeding(static (DbContext context, bool changes) =>
			{
				context.Set<TodoEntity>().AddRange(EnumerateSeed());
				_ = context.SaveChanges();
			})
			.UseAsyncSeeding(static async (DbContext context, bool changes, CancellationToken cancellationToken) =>
			{
				await context.Set<TodoEntity>().AddRangeAsync(EnumerateSeed(), cancellationToken);
				_ = await context.SaveChangesAsync(cancellationToken);
			})
			.Options;
	}

	public TaskListContextFactory(DbContextOptions<TaskListContext> options)
	{
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		_options = options;
	}

	public TaskListContext CreateDbContext()
	{
		return new TaskListContext(_options);
	}

	void IDisposable.Dispose()
	{
		_connection.Dispose();
	}

	async ValueTask IAsyncDisposable.DisposeAsync()
	{
		await _connection.DisposeAsync();
	}

	private static IEnumerable<TodoEntity> EnumerateSeed()
	{
		yield return new TodoEntity
		{
			Id = 1,
			Title = "Walk the dog",
		};
		yield return new TodoEntity
		{
			Id = 2,
			Title = "Do the dishes",
			DueBy = DateOnly.FromDateTime(DateTime.Now),
		};
		yield return new TodoEntity
		{
			Id = 3,
			Title = "Do the laundry",
			DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
		};
		yield return new TodoEntity
		{
			Id = 4,
			Title = "Clean the bathroom",
		};
		yield return new TodoEntity
		{
			Id = 5,
			Title = "Clean the car",
			DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
		};
	}
}
