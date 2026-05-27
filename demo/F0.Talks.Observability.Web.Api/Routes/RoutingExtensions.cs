using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

		todosApi.MapGet("/complete", async Task<Todo[]> (HttpRequest request, [FromServices] TaskListContextFactory dbFactory) =>
			{
				await Tracer.WaitAsync();

				app.Logger.Request(request);

				await using TaskListContext dbContext = dbFactory.CreateDbContext();
				return await dbContext.Todos.Where(static (TodoEntity todo) => todo.IsComplete).Select(static (TodoEntity todo) => Todo.FromDataEntity(todo)).AsAsyncEnumerable().ToArrayAsync(request.HttpContext.RequestAborted);
			})
			.WithName("GetCompleteTodos");

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

		todosApi.MapPost("/", async Task<Created<Todo>> (HttpRequest request, [FromBody] Todo todo, [FromServices] TaskListContextFactory dbFactory) =>
			{
				await Tracer.WaitAsync();

				app.Logger.Request(request);

				await using TaskListContext dbContext = dbFactory.CreateDbContext();
				TodoEntity entity = todo.ToDataEntity();
				await dbContext.Todos.AddAsync(entity, request.HttpContext.RequestAborted);
				await dbContext.SaveChangesAsync(request.HttpContext.RequestAborted);
				todo = Todo.FromDataEntity(entity);
				return TypedResults.Created($"/api/todos/{todo.Id}", todo);
			})
			.WithName("CreateTodo");

		todosApi.MapPut("/{id}", async Task<Results<NoContent, NotFound>> (HttpRequest request, [FromRoute] int id, [FromBody] Todo todo, [FromServices] TaskListContextFactory dbFactory) =>
			{
				await Tracer.WaitAsync();

				app.Logger.Request(request);

				await using TaskListContext dbContext = dbFactory.CreateDbContext();
				TodoEntity? entity = await dbContext.FindByIdAsync(id, request.HttpContext.RequestAborted);
				if (entity is null)
				{
					return TypedResults.NotFound();
				}
				entity.Title = todo.Title;
				entity.DueBy = todo.DueBy;
				entity.IsComplete = todo.IsComplete;
				await dbContext.SaveChangesAsync(request.HttpContext.RequestAborted);
				return TypedResults.NoContent();
			})
			.WithName("UpdateTodo");

		todosApi.MapPatch("/{id}", async Task<Results<NoContent, NotFound>> (HttpRequest request, [FromRoute] int id, [FromBody] Todo todo, [FromServices] TaskListContextFactory dbFactory) =>
			{
				await Tracer.WaitAsync();

				app.Logger.Request(request);

				await using TaskListContext dbContext = dbFactory.CreateDbContext();
				TodoEntity? entity = await dbContext.FindByIdAsync(id, request.HttpContext.RequestAborted);
				if (entity is null)
				{
					return TypedResults.NotFound();
				}
				if (todo.Title is not null)
				{
					entity.Title = todo.Title;
				}
				if (todo.DueBy is not null)
				{
					entity.DueBy = todo.DueBy;
				}
				if (todo.IsComplete)
				{
					entity.IsComplete = todo.IsComplete;
				}
				await dbContext.SaveChangesAsync(request.HttpContext.RequestAborted);
				return TypedResults.NoContent();
			})
			.WithName("PatchTodo");

		todosApi.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (HttpRequest request, [FromRoute] int id, [FromServices] TaskListContextFactory dbFactory) =>
			{
				await Tracer.WaitAsync();

				app.Logger.Request(request);

				await using TaskListContext dbContext = dbFactory.CreateDbContext();
				TodoEntity? entity = await dbContext.FindByIdAsync(id, request.HttpContext.RequestAborted);
				if (entity is null)
				{
					return TypedResults.NotFound();
				}
				dbContext.Todos.Remove(entity);
				await dbContext.SaveChangesAsync(request.HttpContext.RequestAborted);
				return TypedResults.NoContent();
			})
			.WithName("DeleteTodo");
	}
}
