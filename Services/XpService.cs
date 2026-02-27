using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class XpService : IXpService
    {
        private static readonly (int Threshold, string Name)[] LevelTable = new[]
        {
            (0,      "Rookie"),
            (100,    "Gamer"),
            (300,    "Enthusiast"),
            (600,    "Collector"),
            (1100,   "Veteran"),
            (1800,   "Expert"),
            (2800,   "Champion"),
            (4200,   "Master"),
            (6000,   "Legend"),
            (10000,  "Hall of Fame"),
        };

        private readonly ApplicationDbContext _context;

        public XpService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TryGrantAsync(Guid userId, string reason, string referenceId, int amount)
        {
            var alreadyGranted = await _context.XpGrants
                .AnyAsync(g => g.UserId == userId && g.Reason == reason && g.ReferenceId == referenceId);

            if (alreadyGranted)
                return false;

            var grant = new XpGrant
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Reason = reason,
                ReferenceId = referenceId,
                XpAwarded = amount,
                GrantedAt = DateTime.UtcNow
            };

            _context.XpGrants.Add(grant);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _context.ChangeTracker.Clear();
                return false;
            }

            user.XpTotal += amount;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                return false;
            }
        }

        public XpInfoDto ComputeLevel(int xpTotal)
        {
            int levelIndex = 0;
            for (int i = LevelTable.Length - 1; i >= 0; i--)
            {
                if (xpTotal >= LevelTable[i].Threshold)
                {
                    levelIndex = i;
                    break;
                }
            }

            bool isMaxLevel = levelIndex == LevelTable.Length - 1;
            int xpForCurrentLevel = LevelTable[levelIndex].Threshold;
            int xpForNextLevel = isMaxLevel ? xpForCurrentLevel : LevelTable[levelIndex + 1].Threshold;
            int xpIntoCurrentLevel = xpTotal - xpForCurrentLevel;
            int xpNeededForNextLevel = isMaxLevel ? 0 : xpForNextLevel - xpForCurrentLevel;

            double progressPercent = isMaxLevel
                ? 100.0
                : xpNeededForNextLevel > 0
                    ? Math.Min(100.0, (double)xpIntoCurrentLevel / xpNeededForNextLevel * 100.0)
                    : 0.0;

            return new XpInfoDto
            {
                Level = levelIndex + 1,
                LevelName = LevelTable[levelIndex].Name,
                NextLevelName = isMaxLevel ? string.Empty : LevelTable[levelIndex + 1].Name,
                XpTotal = xpTotal,
                XpForCurrentLevel = xpForCurrentLevel,
                XpForNextLevel = xpForNextLevel,
                XpIntoCurrentLevel = xpIntoCurrentLevel,
                XpNeededForNextLevel = xpNeededForNextLevel,
                ProgressPercent = progressPercent,
                IsMaxLevel = isMaxLevel
            };
        }
    }
}
