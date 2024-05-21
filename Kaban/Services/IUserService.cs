using Kaban.Models;

namespace Kaban.Services;

public interface IUserService
{
    public Task<User> Create();
    public Task<User?> Find(string userId);
    public Task DeleteShadowUser(string userId);
    public Task<User?> FindByDiscordId(string discordId);
}