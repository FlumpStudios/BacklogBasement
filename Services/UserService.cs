using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> GetOrCreateUserAsync(string googleSubjectId, string email, string displayName)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleSubjectId == googleSubjectId);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    GoogleSubjectId = googleSubjectId,
                    Email = email,
                    DisplayName = displayName,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return null;

            return await _context.Users.FindAsync(userId);
        }

        public Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            return userId;
        }

        public async Task<User?> LinkSteamAsync(Guid userId, string steamId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            // Check if this Steam ID is already linked to another account
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.SteamId == steamId && u.Id != userId);
            if (existingUser != null)
                throw new InvalidOperationException("This Steam account is already linked to another user");

            user.SteamId = steamId;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> UnlinkSteamAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            user.SteamId = null;
            await _context.SaveChangesAsync();

            return user;
        }
    }
}