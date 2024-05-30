using System.ComponentModel.DataAnnotations;

namespace Kaban.Models;

public enum TaskState
{
    Todo,
    Doing,
    Done,
}

public class MainTask
{
    public int Id { get; set; }
    
    [Required]
    public int Order { get; set; }
    
    [Required]
    public string Title { get; set; } = "Task Name";

    public string Description { get; set; } = "";

    public TaskState Status { get; set; }
    
    public List<SubTask> SubTasks { get; set; } = [];
    
    [GraphQLIgnore]
    public Column Column { get; set; }

    [GraphQLIgnore]
    public int ColumnId { get; set; }
}