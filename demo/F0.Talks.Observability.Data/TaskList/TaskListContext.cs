namespace F0.Talks.Observability.Data.TaskList;

public sealed class TaskListContext : DbContext
{
	public TaskListContext()
	{
	}

	public TaskListContext(DbContextOptions<TaskListContext> options)
		: base(options)
	{
	}

	public DbSet<TodoEntity> Todos { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}
}
