using Kaban.Models;

namespace Kaban.GraphQL.Columns;

public record AddColumnInput(int BoardId, string Name, List<MainTask>? MainTasks = null);