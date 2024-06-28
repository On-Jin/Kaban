namespace Kaban.GraphQL.MainTasks;

public record AddMainTaskInput(
    int ColumnId,
    string Title,
    string? Description = null,
    List<string>? SubTaskTitles = null);