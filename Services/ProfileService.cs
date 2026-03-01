using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFriendshipService _friendshipService;
        private readonly IXpService _xpService;

        public ProfileService(ApplicationDbContext context, IFriendshipService friendshipService, IXpService xpService)
        {
            _context = context;
            _friendshipService = friendshipService;
            _xpService = xpService;
        }

        public async Task<ProfileDto?> GetProfileByUsernameAsync(string username)
        {
            var lowerUsername = username.ToLowerInvariant();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == lowerUsername);

            if (user == null)
                return null;

            var collection = await _context.UserGames
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == user.Id)
                .OrderByDescending(ug => ug.DateAdded)
                .Select(ug => new CollectionItemDto
                {
                    Id = ug.Id,
                    GameId = ug.GameId,
                    GameName = ug.Game.Name,
                    ReleaseDate = ug.Game.ReleaseDate,
                    CoverUrl = ug.Game.CoverUrl,
                    DateAdded = ug.DateAdded,
                    Notes = null, // Don't expose private notes
                    TotalPlayTimeMinutes = ug.PlaySessions.Sum(ps => ps.DurationMinutes),
                    Source = ug.Game.SteamAppId.HasValue ? "steam" : "manual",
                    Status = ug.Status,
                    DateCompleted = ug.DateCompleted,
                    CriticScore = ug.Game.CriticScore
                })
                .ToListAsync();

            var currentlyPlaying = collection.Where(c => c.Status == "playing").ToList();
            var backlog = collection.Where(c => c.Status == "backlog").ToList();

            var friends = await _friendshipService.GetFriendsAsync(user.Id);

            return new ProfileDto
            {
                UserId = user.Id,
                Username = user.Username!,
                DisplayName = user.DisplayName,
                TwitchId = user.TwitchId,
                MemberSince = user.CreatedAt,
                Stats = new ProfileStatsDto
                {
                    TotalGames = collection.Count,
                    TotalPlayTimeMinutes = collection.Sum(c => c.TotalPlayTimeMinutes),
                    BacklogCount = backlog.Count,
                    PlayingCount = currentlyPlaying.Count,
                    CompletedCount = collection.Count(c => c.Status == "completed"),
                    FriendCount = friends.Count
                },
                CurrentlyPlaying = currentlyPlaying,
                Backlog = backlog,
                Collection = collection,
                Friends = friends,
                XpInfo = _xpService.ComputeLevel(user.XpTotal)
            };
        }
    }
}
