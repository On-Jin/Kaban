namespace Kaban.GraphQL.SubTasks;


public record AddSubTasksInput(int MainTaskId, List<string> Title);