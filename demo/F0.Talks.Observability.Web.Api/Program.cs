using System.Text.Json.Serialization;
using F0.Talks.Observability.Data.TaskList;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Sentry.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.WebHost.UseSentry(static (SentryAspNetCoreOptions options) =>
{
	options.Dsn = null;
	options.Debug = true;
	options.SampleRate = 1.0f;
	options.TracesSampleRate = 1.0d;
	options.EnableLogs = true;
	options.EnableMetrics = true;
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();

builder.Services.AddSingleton<TaskListContextFactory>();

builder.Services.AddHostedService<MetricsService>();
builder.Services.AddHostedService<DatabaseService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

var todosApi = app.MapGroup("/api/todos");
todosApi.MapGet("/", async Task<Todo[]> (HttpRequest request, [FromServices] TaskListContextFactory dbFactory) =>
	{
		await Tracer.WaitAsync();

		app.Logger.Request(request);

		await using TaskListContext dbContext = dbFactory.CreateDbContext();
		return await dbContext.All().Select(TodoExtensions.FromDataEntity).ToArrayAsync(request.HttpContext.RequestAborted);
	})
	.WithName("GetTodos");

todosApi.MapGet("/{id}", async Task<Results<Ok<Todo>, NotFound>> (HttpRequest request, [FromRoute] int id, [FromServices] TaskListContextFactory dbFactory) =>
	{
		await Tracer.WaitAsync();

		app.Logger.Request(request);

		await using TaskListContext dbContext = dbFactory.CreateDbContext();
		return await dbContext.FindByIdAsync(id, request.HttpContext.RequestAborted) is { } todo
			? TypedResults.Ok(Todo.FromDataEntity(todo))
			: TypedResults.NotFound();
	})
	.WithName("GetTodoById");

app.Run();

[JsonSerializable(typeof(Todo[]))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
