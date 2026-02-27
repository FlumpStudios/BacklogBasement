using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IXpService _xpService;

        public LeaderboardService(ApplicationDbContext context, IXpService xpService)
        {
            _context = context;
            _xpService = xpService;
        }

        public async Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(Guid currentUserId, int limit = 100)
        {
            var topUsers = await _context.Users
                .OrderByDescending(u => u.XpTotal)
                .ThenBy(u => u.CreatedAt)
                .Take(limit)
                .Select(u => new { u.Id, u.Username, u.DisplayName, u.XpTotal, u.CreatedAt })
                .ToListAsync();

            var entries = topUsers.Select((u, index) =>
            {
                var xpInfo = _xpService.ComputeLevel(u.XpTotal);
                return new LeaderboardEntryDto
                {
                    Rank = index + 1,
                    UserId = u.Id,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    XpTotal = u.XpTotal,
                    Level = xpInfo.Level,
                    LevelName = xpInfo.LevelName,
                    IsCurrentUser = u.Id == currentUserId,
                };
            }).ToList();

            // If current user is outside the top list, append them
            bool currentUserInList = entries.Any(e => e.IsCurrentUser);
            if (!currentUserInList)
            {
                var currentUser = await _context.Users
                    .Where(u => u.Id == currentUserId)
                    .Select(u => new { u.Id, u.Username, u.DisplayName, u.XpTotal, u.CreatedAt })
                    .FirstOrDefaultAsync();

                if (currentUser != null)
                {
                    var userCreatedAt = currentUser.CreatedAt;
                    var rank = await _context.Users
                        .CountAsync(u => u.XpTotal > currentUser.XpTotal ||
                                        (u.XpTotal == currentUser.XpTotal && u.CreatedAt < userCreatedAt));
                    rank += 1;

                    var xpInfo = _xpService.ComputeLevel(currentUser.XpTotal);
                    entries.Add(new LeaderboardEntryDto
                    {
                        Rank = rank,
                        UserId = currentUser.Id,
                        Username = currentUser.Username,
                        DisplayName = currentUser.DisplayName,
                        XpTotal = currentUser.XpTotal,
                        Level = xpInfo.Level,
                        LevelName = xpInfo.LevelName,
                        IsCurrentUser = true,
                    });
                }
            }

            return entries;
        }

        public async Task<List<LeaderboardEntryDto>> GetFriendLeaderboardAsync(Guid currentUserId)
        {
            var friendIds = await _context.Friendships
                .Where(f => f.Status == "accepted" &&
                            (f.RequesterId == currentUserId || f.AddresseeId == currentUserId))
                .Select(f => f.RequesterId == currentUserId ? f.AddresseeId : f.RequesterId)
                .ToListAsync();

            var userIds = friendIds.Append(currentUserId).ToHashSet();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .OrderByDescending(u => u.XpTotal)
                .ThenBy(u => u.CreatedAt)
                .Select(u => new { u.Id, u.Username, u.DisplayName, u.XpTotal })
                .ToListAsync();

            return users.Select((u, index) =>
            {
                var xpInfo = _xpService.ComputeLevel(u.XpTotal);
                return new LeaderboardEntryDto
                {
                    Rank = index + 1,
                    UserId = u.Id,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    XpTotal = u.XpTotal,
                    Level = xpInfo.Level,
                    LevelName = xpInfo.LevelName,
                    IsCurrentUser = u.Id == currentUserId,
                };
            }).ToList();
        }
    }
}
