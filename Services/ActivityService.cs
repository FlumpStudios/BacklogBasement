using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(ApplicationDbContext context, ILogger<ActivityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(Guid userId, string eventType, Guid? gameId = null, Guid? clubId = null, int? intValue = null)
        {
            try
            {
                var evt = new ActivityEvent
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EventType = eventType,
                    GameId = gameId,
                    ClubId = clubId,
                    IntValue = intValue,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ActivityEvents.Add(evt);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log activity event {EventType} for user {UserId}", eventType, userId);
            }
        }

        public async Task<IEnumerable<ActivityEventDto>> GetFeedAsync(int limit = 50)
        {
            var events = await _context.ActivityEvents
                .Include(e => e.User)
                .Include(e => e.Game)
                .Include(e => e.Club)
                .OrderByDescending(e => e.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return events.Select(e => new ActivityEventDto
            {
                Id = e.Id,
                UserId = e.UserId,
                Username = e.User.Username ?? string.Empty,
                DisplayName = e.User.DisplayName,
                UserAvatarUrl = e.User.AvatarUrl,
                EventType = e.EventType,
                GameId = e.GameId,
                GameName = e.Game?.Name,
                GameCoverUrl = e.Game?.CoverUrl,
                ClubId = e.ClubId,
                ClubName = e.Club?.Name,
                IntValue = e.IntValue,
                CreatedAt = e.CreatedAt
            });
        }
    }
}
