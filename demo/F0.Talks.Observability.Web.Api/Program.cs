using System.Text.Json.Serialization;
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

builder.Services.AddHostedService<MetricsService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

Todo[] sampleTodos =
[
	new(1, "Walk the dog"),
	new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
	new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
	new(4, "Clean the bathroom"),
	new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
];

var todosApi = app.MapGroup("/api/todos");
todosApi.MapGet("/", async Task<Todo[]> (HttpRequest request) =>
	{
		await Tracer.WaitAsync();

		app.Logger.Request(request);

		return sampleTodos;
	})
	.WithName("GetTodos");

todosApi.MapGet("/{id}", async Task<Results<Ok<Todo>, NotFound>> (HttpRequest request, [FromRoute] int id) =>
	{
		await Tracer.WaitAsync();

		app.Logger.Request(request);

		return sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
			? TypedResults.Ok(todo)
			: TypedResults.NotFound();
	})
	.WithName("GetTodoById");

app.Run();

public sealed record class Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
