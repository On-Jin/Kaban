using Kaban.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaban.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Column> Columns { get; set; }

    public DbSet<MainTask> MainTasks { get; set; }

    public DbSet<SubTask> SubTasks { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }

    public DbSet<User> Users { get; set; }

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

        // ##########

        modelBuilder.Entity<User>()
            .HasMany(e => e.Boards)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<Board>()
        //     .HasOne(e => e.User)
        //     .WithMany(e => e.Boards)
        //     .HasForeignKey(e => e.UserId)
        //     .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Board>()
            .HasMany(e => e.Columns)
            .WithOne(e => e.Board)
            .HasForeignKey(e => e.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Column>()
            .HasMany(e => e.MainTasks)
            .WithOne(e => e.Column)
            .HasForeignKey(e => e.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MainTask>()
            .HasMany(e => e.SubTasks)
            .WithOne(e => e.MainTask)
            .HasForeignKey(e => e.MainTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}