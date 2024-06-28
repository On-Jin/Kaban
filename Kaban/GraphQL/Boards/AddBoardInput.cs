namespace Kaban.GraphQL.Boards;

public record AddBoardInput(string Name, List<string>? ColumnNames = null);