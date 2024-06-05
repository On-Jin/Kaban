using System.ComponentModel.DataAnnotations;

namespace Kaban.Models;

public class Column : IOrdered
{
    public int Id { get; set; }

    [Required]
    public int Order { get; set; }
    
    [Required]
    public string Name { get; set; } = "Column Name";

    public List<MainTask> MainTasks { get; set; } = [];
    
    [GraphQLIgnore]
    public Board Board { get; set; }

    [GraphQLIgnore]
    public int BoardId { get; set; }
}