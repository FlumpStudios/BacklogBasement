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

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
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
                    DateCompleted = ug.DateCompleted
                })
                .ToListAsync();

            var currentlyPlaying = collection.Where(c => c.Status == "playing").ToList();
            var backlog = collection.Where(c => c.Status == "backlog").ToList();

            return new ProfileDto
            {
                Username = user.Username!,
                DisplayName = user.DisplayName,
                MemberSince = user.CreatedAt,
                Stats = new ProfileStatsDto
                {
                    TotalGames = collection.Count,
                    TotalPlayTimeMinutes = collection.Sum(c => c.TotalPlayTimeMinutes),
                    BacklogCount = backlog.Count,
                    PlayingCount = currentlyPlaying.Count,
                    CompletedCount = collection.Count(c => c.Status == "completed")
                },
                CurrentlyPlaying = currentlyPlaying,
                Backlog = backlog,
                Collection = collection
            };
        }
    }
}
