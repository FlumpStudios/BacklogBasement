using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.Exceptions;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfanityService _profanityService;

        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IProfanityService profanityService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _profanityService = profanityService;
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
        public Guid? GetCachedCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            return userId;
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

        private static readonly HashSet<string> ReservedUsernames = new(StringComparer.OrdinalIgnoreCase)
        {
            "admin", "api", "auth", "profile", "dashboard", "collection",
            "search", "games", "settings", "help", "about", "login", "logout",
            "register", "signup", "signin", "user", "users", "home", "privacy",
            "cookies", "terms", "support", "contact", "blog", "news", "status",
            "friends", "notifications"
        };

        private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9][a-zA-Z0-9_-]*[a-zA-Z0-9]$", RegexOptions.Compiled);

        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new BadRequestException("Username is required");

            if (username.Length < 3)
                throw new BadRequestException("Username must be at least 3 characters");

            if (username.Length > 30)
                throw new BadRequestException("Username must be at most 30 characters");

            if (!UsernameRegex.IsMatch(username))
                throw new BadRequestException("Username can only contain letters, numbers, hyphens, and underscores, and cannot start or end with a hyphen or underscore");

            if (ReservedUsernames.Contains(username))
                throw new BadRequestException("This username is reserved");
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            ValidateUsername(username);

            var lowerUsername = username.ToLowerInvariant();
            return !await _context.Users
                .AnyAsync(u => u.Username != null && u.Username.ToLower() == lowerUsername);
        }

        public async Task<User> SetUsernameAsync(Guid userId, string username)
        {
            ValidateUsername(username);
            _profanityService.AssertClean(username, "Username");

            var user = await _context.Users.FindAsync(userId)
                ?? throw new NotFoundException("User not found");

            if (user.Username != null)
                throw new BadRequestException("Username has already been set and cannot be changed");

            var lowerUsername = username.ToLowerInvariant();
            var taken = await _context.Users
                .AnyAsync(u => u.Username != null && u.Username.ToLower() == lowerUsername);

            if (taken)
                throw new BadRequestException("This username is already taken");

            user.Username = username;
            await _context.SaveChangesAsync();

            return user;
        }
    }
}