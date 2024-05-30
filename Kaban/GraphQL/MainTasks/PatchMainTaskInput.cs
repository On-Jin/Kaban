using Kaban.Models;

namespace Kaban.GraphQL.MainTasks;

public record PatchMainTaskInput(int Id, string? Status, string? Title, string? Description);