using Kaban.Data;
using Kaban.Models;

namespace Kaban.Query;

public class Query
{
    [UseProjection]
    public IQueryable<Author> GetAuthors(AppDbContext appDbContext)
    {
        return appDbContext.Authors;
    }
    
    public Book GetBook() =>
        new Book
        {
            Title = "C# in depth.",
            Author = new Author
            {
                Name = "Jon Skeet"
            }
        };
}