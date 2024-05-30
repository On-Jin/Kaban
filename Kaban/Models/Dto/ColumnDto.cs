namespace Kaban.Models.Dto;

public class ColumnDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public List<MainTaskDto> MainTasks { get; set; } = [];
}