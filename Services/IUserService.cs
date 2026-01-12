using System;
using System.Threading.Tasks;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public interface IUserService
    {
        Task<User> GetOrCreateUserAsync(string googleSubjectId, string email, string displayName);
        Task<User?> GetCurrentUserAsync();
        Guid? GetCurrentUserId();
        Task<User?> LinkSteamAsync(Guid userId, string steamId);
        Task<User?> UnlinkSteamAsync(Guid userId);
    }
}