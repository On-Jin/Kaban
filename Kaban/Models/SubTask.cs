using System.ComponentModel.DataAnnotations;

namespace Kaban.Models;

public class SubTask
{
    public int Id { get; set; }

    [Required]
    public int Order { get; set; }
    
    [Required]
    public string Title { get; set; } = "Subtask Name";

    public bool IsCompleted { get; set; } = false;

    [GraphQLIgnore]
    public MainTask MainTask { get; set; }

    [GraphQLIgnore]
    public int MainTaskId { get; set; }
}