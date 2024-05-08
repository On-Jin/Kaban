using System.ComponentModel.DataAnnotations;

namespace Kaban.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    public string? DiscordId { get; set; } = null;
}