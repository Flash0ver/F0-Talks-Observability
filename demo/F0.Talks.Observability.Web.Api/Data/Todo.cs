namespace F0.Talks.Observability.Web.Api.Data;

internal sealed record class Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
