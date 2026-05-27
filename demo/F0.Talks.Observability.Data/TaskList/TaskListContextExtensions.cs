namespace F0.Talks.Observability.Data.TaskList;

public static class TaskListContextExtensions
{
	private static readonly Func<TaskListContext, IAsyncEnumerable<TodoEntity>> s_all
		= EF.CompileAsyncQuery(static (TaskListContext context) => context.Todos);

	private static readonly Func<TaskListContext, int, IAsyncEnumerable<TodoEntity?>> s_findByIdAsync
		= EF.CompileAsyncQuery(static (TaskListContext context, int id) => context.Todos.Where((TodoEntity todo) => todo.Id == id));

	extension(TaskListContext context)
	{
		public IAsyncEnumerable<TodoEntity> All()
		{
			return s_all.Invoke(context);
		}

		public ValueTask<TodoEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return s_findByIdAsync.Invoke(context, id).FirstOrDefaultAsync(cancellationToken);
		}
	}
}
