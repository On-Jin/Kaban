using System.ComponentModel.DataAnnotations;

namespace Kaban.Models;


public class Author
{
    [GraphQLIgnore]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public List<Book> Books { get; set; } = [];
}