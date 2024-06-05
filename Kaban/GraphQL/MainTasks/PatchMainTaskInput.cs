using Kaban.Models;

namespace Kaban.GraphQL.MainTasks;

public record PatchMainTaskInput(int Id, string? Title, string? Description);

public record MoveMainTaskInput(int Id, string? Status, int? Order);