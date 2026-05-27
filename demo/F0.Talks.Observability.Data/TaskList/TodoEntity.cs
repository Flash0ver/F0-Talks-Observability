namespace F0.Talks.Observability.Data.TaskList;

public sealed class TodoEntity
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public DateOnly? DueBy { get; set; }
	public bool IsComplete { get; set; }
}
