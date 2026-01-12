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
        private readonly IIgdbService _igdbService;
        private readonly ILogger<SteamImportService> _logger;

        public SteamImportService(
            ApplicationDbContext context,
            ISteamService steamService,
            IIgdbService igdbService,
            ILogger<SteamImportService> logger)
        {
            _context = context;
            _steamService = steamService;
            _igdbService = igdbService;
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

            foreach (var steamGame in steamGamesList)
            {
                try
                {
                    // Check if game already exists by SteamAppId
                    var existingGame = await _context.Games
                        .FirstOrDefaultAsync(g => g.SteamAppId == steamGame.AppId);

                    Game game;

                    if (existingGame != null)
                    {
                        game = existingGame;
                    }
                    else
                    {
                        // Try to match to IGDB by name
                        game = await CreateOrMatchGameAsync(steamGame);
                    }

                    // Check if already in user's collection
                    var existingUserGame = await _context.UserGames
                        .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == game.Id);

                    if (existingUserGame != null)
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
                        Notes = $"Imported from Steam"
                    };

                    _context.UserGames.Add(userGame);

                    // Import playtime if requested
                    int? importedPlaytime = null;
                    if (includePlaytime && steamGame.PlaytimeForever > 0)
                    {
                        var playSession = new PlaySession
                        {
                            Id = Guid.NewGuid(),
                            UserGameId = userGame.Id,
                            DurationMinutes = steamGame.PlaytimeForever,
                            PlayedAt = DateTime.UtcNow // We don't have exact date, use import date
                        };
                        _context.PlaySessions.Add(playSession);
                        importedPlaytime = steamGame.PlaytimeForever;
                    }

                    await _context.SaveChangesAsync();

                    result.ImportedCount++;
                    result.ImportedGames.Add(new SteamImportedGameDto
                    {
                        GameId = game.Id,
                        Name = game.Name,
                        SteamAppId = steamGame.AppId,
                        IgdbId = game.IgdbId,
                        MatchedToIgdb = game.IgdbId.HasValue,
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

            _logger.LogInformation(
                "Steam import complete for user {UserId}: {Imported} imported, {Skipped} skipped, {Failed} failed",
                userId, result.ImportedCount, result.SkippedCount, result.FailedCount);

            return result;
        }

        private async Task<Game> CreateOrMatchGameAsync(SteamGame steamGame)
        {
            // Try to find an IGDB match by name
            IgdbGame? igdbMatch = null;

            try
            {
                var igdbResults = await _igdbService.SearchGamesAsync(steamGame.Name);

                // Try exact match first
                igdbMatch = igdbResults.FirstOrDefault(g =>
                    string.Equals(g.Name, steamGame.Name, StringComparison.OrdinalIgnoreCase));

                // If no exact match, try fuzzy match (first result if it's close enough)
                if (igdbMatch == null)
                {
                    var firstResult = igdbResults.FirstOrDefault();
                    if (firstResult != null && IsFuzzyMatch(steamGame.Name, firstResult.Name))
                    {
                        igdbMatch = firstResult;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IGDB search failed for game: {GameName}", steamGame.Name);
            }

            // Check if IGDB game already exists in our database
            if (igdbMatch != null)
            {
                var existingIgdbGame = await _context.Games
                    .FirstOrDefaultAsync(g => g.IgdbId == igdbMatch.Id);

                if (existingIgdbGame != null)
                {
                    // Update existing game with Steam App ID if not already set
                    if (!existingIgdbGame.SteamAppId.HasValue)
                    {
                        existingIgdbGame.SteamAppId = steamGame.AppId;
                        await _context.SaveChangesAsync();
                    }
                    return existingIgdbGame;
                }
            }

            // Create new game
            var game = new Game
            {
                Id = Guid.NewGuid(),
                SteamAppId = steamGame.AppId,
                Name = steamGame.Name,
                Summary = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            // If we have an IGDB match, use its data
            if (igdbMatch != null)
            {
                game.IgdbId = igdbMatch.Id;
                game.Summary = igdbMatch.Summary ?? string.Empty;

                if (igdbMatch.first_release_date.HasValue)
                {
                    game.ReleaseDate = DateTimeOffset.FromUnixTimeSeconds(igdbMatch.first_release_date.Value).DateTime;
                }

                if (igdbMatch.Cover != null && !string.IsNullOrEmpty(igdbMatch.Cover.image_id))
                {
                    game.CoverUrl = $"https://images.igdb.com/igdb/image/upload/t_cover_big/{igdbMatch.Cover.image_id}.jpg";
                }
            }
            else
            {
                // Use Steam image if available
                if (!string.IsNullOrEmpty(steamGame.ImgLogoUrl))
                {
                    game.CoverUrl = steamGame.ImgLogoUrl;
                }
            }

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return game;
        }

        private bool IsFuzzyMatch(string steamName, string igdbName)
        {
            // Simple fuzzy matching: normalize and compare
            var normalizedSteam = NormalizeName(steamName);
            var normalizedIgdb = NormalizeName(igdbName);

            // Exact match after normalization
            if (normalizedSteam == normalizedIgdb)
                return true;

            // Check if one contains the other
            if (normalizedSteam.Contains(normalizedIgdb) || normalizedIgdb.Contains(normalizedSteam))
                return true;

            // Calculate similarity (simple Levenshtein-like check)
            var similarity = CalculateSimilarity(normalizedSteam, normalizedIgdb);
            return similarity > 0.8; // 80% similarity threshold
        }

        private string NormalizeName(string name)
        {
            return name
                .ToLowerInvariant()
                .Replace(":", "")
                .Replace("-", " ")
                .Replace("'", "")
                .Replace("'", "")
                .Replace("™", "")
                .Replace("®", "")
                .Trim();
        }

        private double CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;

            var maxLength = Math.Max(s1.Length, s2.Length);
            if (maxLength == 0)
                return 1.0;

            var distance = LevenshteinDistance(s1, s2);
            return 1.0 - ((double)distance / maxLength);
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            var n = s1.Length;
            var m = s2.Length;
            var d = new int[n + 1, m + 1];

            for (var i = 0; i <= n; i++)
                d[i, 0] = i;
            for (var j = 0; j <= m; j++)
                d[0, j] = j;

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}
