using System.ComponentModel.DataAnnotations;

namespace Kaban.Model;


public class Author
{
    [GraphQLIgnore]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public List<Book> Books { get; set; } = [];
}