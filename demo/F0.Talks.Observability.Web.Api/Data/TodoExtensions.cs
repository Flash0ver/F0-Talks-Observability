namespace F0.Talks.Observability.Web.Api.Data;

internal static class TodoExtensions
{
	extension(Todo todo)
	{
		internal TodoEntity ToDataEntity()
		{
			return new TodoEntity
			{
				Id = todo.Id,
				Title = todo.Title,
				DueBy = todo.DueBy,
				IsComplete = todo.IsComplete,
			};
		}
	}

	extension(Todo)
	{
		internal static Todo FromDataEntity(TodoEntity todo)
		{
			return new Todo(todo.Id, todo.Title, todo.DueBy, todo.IsComplete);
		}
	}
}
