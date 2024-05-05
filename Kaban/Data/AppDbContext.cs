using Kaban.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaban.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>()
            .HasMany(e => e.Books)
            .WithOne(e => e.Author)
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Book>()
            .HasOne(e => e.Author)
            .WithMany(e => e.Books)
            .HasForeignKey(e => e.AuthorId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);
    }
}