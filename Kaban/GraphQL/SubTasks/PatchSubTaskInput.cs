namespace Kaban.GraphQL.SubTasks;

public record PatchSubTaskInput(int Id, string? Title, bool? IsCompleted);