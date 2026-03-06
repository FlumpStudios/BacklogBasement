using System;
using System.Threading.Tasks;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public interface IUserService
    {
        Task<User> GetOrCreateUserAsync(string googleSubjectId, string displayName, string? avatarUrl = null);
        Task<User> GetOrCreateSteamUserAsync(string steamId, string displayName, string? avatarUrl = null);
        Task<User> GetOrCreateTwitchUserAsync(string twitchId, string displayName, string? avatarUrl = null);
        Task<User?> GetCurrentUserAsync();
        Guid? GetCurrentUserId();
        Guid? GetCachedCurrentUserId();
        Task<User?> LinkSteamAsync(Guid userId, string steamId);
        Task<User?> UnlinkSteamAsync(Guid userId);
        Task<User?> LinkTwitchAsync(Guid userId, string twitchId);
        Task<User?> UnlinkTwitchAsync(Guid userId);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<User> SetUsernameAsync(Guid userId, string username);
    }
}