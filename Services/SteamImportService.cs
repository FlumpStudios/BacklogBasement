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
    public class SteamImportService : ISteamImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISteamService _steamService;
        private readonly ILogger<SteamImportService> _logger;

        public SteamImportService(
            ApplicationDbContext context,
            ISteamService steamService,
            ILogger<SteamImportService> logger)
        {
            _context = context;
            _steamService = steamService;
            _logger = logger;
        }

        public async Task<SteamStatusDto> GetSteamStatusAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return new SteamStatusDto
            {
                IsLinked = !string.IsNullOrEmpty(user?.SteamId),
                SteamId = user?.SteamId
            };
        }

        public async Task<SteamImportResult> ImportLibraryAsync(Guid userId, bool includePlaytime)
        {
            var result = new SteamImportResult();

            // Get user and verify Steam is linked
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.SteamId))
            {
                throw new InvalidOperationException("Steam account is not linked");
            }

            // Fetch games from Steam
            var steamGames = await _steamService.GetOwnedGamesAsync(user.SteamId);
            var steamGamesList = steamGames.ToList();
            result.TotalGames = steamGamesList.Count;

            _logger.LogInformation("Importing {Count} games from Steam for user {UserId}", result.TotalGames, userId);

            // Get all existing games by SteamAppId in one query for efficiency
            var steamAppIds = steamGamesList.Select(g => g.AppId).ToList();
            var existingGames = await _context.Games
                .Where(g => g.SteamAppId.HasValue && steamAppIds.Contains(g.SteamAppId.Value))
                .ToDictionaryAsync(g => g.SteamAppId!.Value, g => g);

            // Get user's existing collection game IDs
            var existingCollectionGameIds = await _context.UserGames
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GameId)
                .ToHashSetAsync();

            foreach (var steamGame in steamGamesList)
            {
                try
                {
                    Game game;

                    // Check if game already exists by SteamAppId
                    if (existingGames.TryGetValue(steamGame.AppId, out var existingGame))
                    {
                        game = existingGame;
                    }
                    else
                    {
                        // Create new game with Steam data only
                        game = new Game
                        {
                            Id = Guid.NewGuid(),
                            SteamAppId = steamGame.AppId,
                            Name = steamGame.Name,
                            Summary = string.Empty,
                            CoverUrl = steamGame.HeaderUrl, // Use Steam CDN header image
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Games.Add(game);
                        existingGames[steamGame.AppId] = game; // Track for duplicates in same batch
                    }

                    // Check if already in user's collection
                    if (existingCollectionGameIds.Contains(game.Id))
                    {
                        result.SkippedCount++;
                        result.SkippedGames.Add(new SteamSkippedGameDto
                        {
                            Name = steamGame.Name,
                            SteamAppId = steamGame.AppId,
                            Reason = "Already in collection"
                        });
                        continue;
                    }

                    // Add to collection
                    var userGame = new UserGame
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        GameId = game.Id,
                        DateAdded = DateTime.UtcNow,
                        Notes = "Imported from Steam"
                    };

                    _context.UserGames.Add(userGame);
                    existingCollectionGameIds.Add(game.Id); // Track for duplicates in same batch

                    // Import playtime if requested
                    int? importedPlaytime = null;
                    if (includePlaytime && steamGame.PlaytimeForever > 0)
                    {
                        var playSession = new PlaySession
                        {
                            Id = Guid.NewGuid(),
                            UserGameId = userGame.Id,
                            DurationMinutes = steamGame.PlaytimeForever,
                            PlayedAt = DateTime.UtcNow
                        };
                        _context.PlaySessions.Add(playSession);
                        importedPlaytime = steamGame.PlaytimeForever;
                    }

                    result.ImportedCount++;
                    result.ImportedGames.Add(new SteamImportedGameDto
                    {
                        GameId = game.Id,
                        Name = game.Name,
                        SteamAppId = steamGame.AppId,
                        IgdbId = null,
                        MatchedToIgdb = false,
                        PlaytimeMinutes = importedPlaytime
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import Steam game: {GameName} ({AppId})", steamGame.Name, steamGame.AppId);
                    result.FailedCount++;
                    result.FailedGames.Add(new SteamFailedGameDto
                    {
                        Name = steamGame.Name,
                        SteamAppId = steamGame.AppId,
                        Error = ex.Message
                    });
                }
            }

            // Save all changes in one batch
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Steam import complete for user {UserId}: {Imported} imported, {Skipped} skipped, {Failed} failed",
                userId, result.ImportedCount, result.SkippedCount, result.FailedCount);

            return result;
        }
    }
}
