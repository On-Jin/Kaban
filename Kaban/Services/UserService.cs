﻿using Kaban.Data;
using Kaban.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaban.Services;

public class UserService(AppDbContext dbContext) : IUserService
{
    public async Task<User> Create()
    {
        var user = new User();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> Find(string userId)
    {
        return await dbContext.Users.FindAsync(new Guid(userId));
    }

    public async Task DeleteShadowUser(string userId)
    {
        var user = await dbContext.Users.FindAsync(new Guid(userId));
        if (user.DiscordId != null)
            throw new Exception($"{user} is not a ShadowUser, he cant be destroyed.");
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<User?> FindByDiscordId(string discordId)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
    }
}