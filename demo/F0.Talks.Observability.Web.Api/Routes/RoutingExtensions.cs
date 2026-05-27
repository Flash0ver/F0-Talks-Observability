using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace F0.Talks.Observability.Web.Api.Routes;

internal static class RoutingExtensions
{
	extension(WebApplication app)
	{
		internal void MapRoutes()
		{
			Router.Map(app);
		}
	}
}

file static class Router
{
	internal static void Map(WebApplication app)
	{
		app.MapGet("/", () => "Hello, World!");
		app.MapGet("/api/", () => "Hello, World!");

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
	}
}
