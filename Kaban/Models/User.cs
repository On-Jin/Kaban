using System.ComponentModel.DataAnnotations;

namespace Kaban.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    public string? DiscordId { get; set; } = null;

    public string? DiscordAvatar { get; set; } = null;

    public string? DiscordUsername { get; set; } = null;

    public List<Board> Boards { get; set; } = [];
}