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

            // Get user's existing collection, mapping GameId -> UserGameId
            var existingUserGameIds = await _context.UserGames
                .Where(ug => ug.UserId == userId)
                .ToDictionaryAsync(ug => ug.GameId, ug => ug.Id);

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
                    if (existingUserGameIds.TryGetValue(game.Id, out var existingUserGameId))
                    {
                        // Upgrade: sync playtime for games already in the collection
                        if (includePlaytime && steamGame.PlaytimeForever > 0)
                        {
                            var existingSessions = await _context.PlaySessions
                                .Where(ps => ps.UserGameId == existingUserGameId)
                                .ToListAsync();
                            _context.PlaySessions.RemoveRange(existingSessions);
                            _context.PlaySessions.Add(new PlaySession
                            {
                                Id = Guid.NewGuid(),
                                UserGameId = existingUserGameId,
                                DurationMinutes = steamGame.PlaytimeForever,
                                PlayedAt = DateTime.UtcNow
                            });
                            result.UpdatedCount++;
                        }
                        else
                        {
                            result.SkippedCount++;
                            result.SkippedGames.Add(new SteamSkippedGameDto
                            {
                                Name = steamGame.Name,
                                SteamAppId = steamGame.AppId,
                                Reason = "Already in collection"
                            });
                        }
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
                    existingUserGameIds[game.Id] = userGame.Id; // Track for duplicates in same batch

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

        public async Task<SteamPlaytimeSyncResult> SyncGamePlaytimeAsync(Guid userId, Guid gameId)
        {
            // Get user and verify Steam is linked
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.SteamId))
            {
                return new SteamPlaytimeSyncResult
                {
                    Success = false,
                    Error = "Steam account is not linked"
                };
            }

            // Get the game and verify it has a SteamAppId
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return new SteamPlaytimeSyncResult
                {
                    Success = false,
                    Error = "Game not found"
                };
            }

            if (!game.SteamAppId.HasValue)
            {
                return new SteamPlaytimeSyncResult
                {
                    Success = false,
                    Error = "Game is not a Steam game"
                };
            }

            // Verify game is in user's collection
            var userGame = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame == null)
            {
                return new SteamPlaytimeSyncResult
                {
                    Success = false,
                    Error = "Game is not in your collection"
                };
            }

            // Fetch playtime from Steam
            var playtime = await _steamService.GetGamePlaytimeAsync(user.SteamId, game.SteamAppId.Value);
            if (playtime == null)
            {
                return new SteamPlaytimeSyncResult
                {
                    Success = false,
                    Error = "Could not fetch playtime from Steam"
                };
            }

            // Delete all existing play sessions for this game
            var existingSessions = await _context.PlaySessions
                .Where(ps => ps.UserGameId == userGame.Id)
                .ToListAsync();

            _context.PlaySessions.RemoveRange(existingSessions);

            // Create new play session with Steam's total playtime (if > 0)
            if (playtime.Value > 0)
            {
                var playSession = new PlaySession
                {
                    Id = Guid.NewGuid(),
                    UserGameId = userGame.Id,
                    DurationMinutes = playtime.Value,
                    PlayedAt = DateTime.UtcNow
                };
                _context.PlaySessions.Add(playSession);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Synced Steam playtime for game {GameId}: {Playtime} minutes",
                gameId, playtime.Value);

            return new SteamPlaytimeSyncResult
            {
                Success = true,
                PlaytimeMinutes = playtime.Value
            };
        }

        public async Task<SteamBulkPlaytimeSyncResult> SyncAllPlaytimesAsync(Guid userId)
        {
            var result = new SteamBulkPlaytimeSyncResult();

            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.SteamId))
            {
                throw new InvalidOperationException("Steam account is not linked");
            }

            // Fetch all games from Steam in one API call
            var steamGames = await _steamService.GetOwnedGamesAsync(user.SteamId);
            var steamPlaytimeByAppId = steamGames.ToDictionary(g => g.AppId, g => g.PlaytimeForever);

            // Get all Steam games in the user's collection
            var userSteamGames = await _context.UserGames
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == userId && ug.Game.SteamAppId.HasValue)
                .ToListAsync();

            result.TotalGames = userSteamGames.Count;

            foreach (var userGame in userSteamGames)
            {
                try
                {
                    var steamAppId = userGame.Game.SteamAppId!.Value;
                    if (!steamPlaytimeByAppId.TryGetValue(steamAppId, out var playtime))
                        continue;

                    // Delete existing play sessions for this game
                    var existingSessions = await _context.PlaySessions
                        .Where(ps => ps.UserGameId == userGame.Id)
                        .ToListAsync();

                    _context.PlaySessions.RemoveRange(existingSessions);

                    // Create new play session with Steam's total playtime
                    if (playtime > 0)
                    {
                        var playSession = new PlaySession
                        {
                            Id = Guid.NewGuid(),
                            UserGameId = userGame.Id,
                            DurationMinutes = playtime,
                            PlayedAt = DateTime.UtcNow
                        };
                        _context.PlaySessions.Add(playSession);
                    }

                    result.UpdatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync playtime for game {GameId}", userGame.GameId);
                    result.FailedCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk playtime sync complete for user {UserId}: {Updated} updated, {Failed} failed out of {Total}",
                userId, result.UpdatedCount, result.FailedCount, result.TotalGames);

            return result;
        }
    }
}
