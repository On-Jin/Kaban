namespace Kaban.Models.Dto;

public class BoardDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "Board Name";

    public List<ColumnDto> Columns { get; set; } = [];
}

public static class Mapper
{
    public static SubTaskDto MapToSubTaskDto(SubTask subTask)
    {
        return new SubTaskDto
        {
            Id = subTask.MainTaskId,
            Title = subTask.Title,
            IsCompleted = subTask.IsCompleted
        };
    }

    public static MainTaskDto MapToMainTaskDto(MainTask mainTask)
    {
        return new MainTaskDto
        {
            Id = mainTask.Id,
            Title = mainTask.Title,
            SubTasks = mainTask.SubTasks.Select(MapToSubTaskDto).ToList()
        };
    }

    public static ColumnDto MapToColumnDto(Column column)
    {
        return new ColumnDto
        {
            Id = column.Id,
            Name = column.Name,
            MainTasks = column.MainTasks.Select(MapToMainTaskDto).ToList()
        };
    }

    public static BoardDto MapToBoardDto(Board board)
    {
        return new BoardDto
        {
            Id = board.Id,
            Name = board.Name,
            Columns = board.Columns.Select(MapToColumnDto).ToList(),
        };
    }
}