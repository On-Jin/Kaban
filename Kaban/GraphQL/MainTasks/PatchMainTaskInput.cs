using Kaban.Models;

namespace Kaban.GraphQL.MainTasks;

public record PatchMainTaskInput(int Id, TaskState? Status, string? Title, string? Description);