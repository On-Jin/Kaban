using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kaban.Models;

public class Board
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "Board Name";

    public List<Column> Columns { get; set; } = [];

    [GraphQLIgnore]
    [JsonIgnore]
    public User User { get; set; }

    [GraphQLIgnore]
    [JsonIgnore]
    public Guid UserId { get; set; }
}