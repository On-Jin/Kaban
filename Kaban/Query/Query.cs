using System.Security.Claims;
using HotChocolate.Authorization;
using Kaban.Data;
using Kaban.Models;
using Kaban.Models.Dto;
using Kaban.Services;
using Microsoft.EntityFrameworkCore;

namespace Kaban.Query;

public class Query
{
    public Query()
    {
    }

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

    [Authorize(Policy = "discord-enabled")]
    public Book GetBookAuth() =>
        new Book
        {
            Title = "C# in depth.",
            Author = new Author
            {
                Name = "Jon Skeet"
            }
        };

    [Authorize]
    public async Task<Me> Me([Service] IHttpContextAccessor httpContext, [Service] IUserService userService)
    {
        Console.WriteLine("Me!");
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        return new Me()
        {
            Id = user.Id!,
            DiscordUsername = user.DiscordUsername,
            DiscordAvatarUrl = user.DiscordId != null
                ? $"https://cdn.discordapp.com/avatars/{user.DiscordId}/{user.DiscordAvatar}"
                : null
        };
    }

    [Authorize]
    public async Task<List<BoardDto>> GetBoards(
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService
    )
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var boardsDto = await db.Entry(user)
            .Collection(u => u.Boards)
            .Query()
            .Include(b => b.Columns)
            .ThenInclude(c => c.MainTasks)
            .ThenInclude(m => m.SubTasks)
            .Select(board =>
                new BoardDto()
                {
                    Id = board.Id,
                    Name = board.Name,
                    Columns = board.Columns.Select(column => new ColumnDto
                    {
                        Id = column.Id,
                        Name = column.Name,
                        MainTasks = column.MainTasks.Select(mainTask => new MainTaskDto
                        {
                            Id = mainTask.Id,
                            Title = mainTask.Title,
                            Description = mainTask.Description,
                            Status = column.Name,
                            SubTasks = mainTask.SubTasks.Select(subTask => new SubTaskDto
                            {
                                Id = subTask.Id,
                                Title = subTask.Title,
                                IsCompleted = subTask.IsCompleted,
                            }).ToList()
                        }).ToList()
                    }).ToList()
                })
            .ToListAsync();

        return boardsDto;
    }
}