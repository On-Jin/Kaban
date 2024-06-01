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

        var boards = await db.Entry(user)
            .Collection(u => u.Boards)
            .Query()
            .Include(b => b.Columns)
            .ThenInclude(c => c.MainTasks)
            .ThenInclude(m => m.SubTasks)
            .ToListAsync();

        return Mapper.MapToBoardsDto(boards).ToList();
    }
}