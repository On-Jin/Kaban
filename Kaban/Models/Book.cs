using System.ComponentModel.DataAnnotations;

namespace Kaban.Models;

public class Book
{
    [GraphQLIgnore]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }
    
    [GraphQLIgnore]
    public Author Author { get; set; }

    [GraphQLIgnore]
    public int AuthorId { get; set; }
}
