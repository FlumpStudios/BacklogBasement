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
    public class GameService : IGameService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIgdbService _igdbService;
        private readonly ISteamService _steamService;

        public GameService(ApplicationDbContext context, IIgdbService igdbService, ISteamService steamService)
        {
            _context = context;
            _igdbService = igdbService;
            _steamService = steamService;
        }

        public async Task<IEnumerable<GameSummaryDto>> SearchGamesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<GameSummaryDto>();

            // First search local database
            var localGames = await _context.Games
                .Where(g => g.Name.ToLower().Contains(query.ToLower()))
                .Select(g => new GameSummaryDto
                {
                    Id = g.Id,
                    IgdbId = g.IgdbId,
                    SteamAppId = g.SteamAppId,
                    Name = g.Name,
                    ReleaseDate = g.ReleaseDate,
                    CoverUrl = g.CoverUrl,
                    CriticScore = g.CriticScore
                })
                .ToListAsync();

            // If not enough results, search IGDB
            if (localGames.Count < 5)
            {
                var igdbGames = await _igdbService.SearchGamesAsync(query);
                System.Console.WriteLine($"IGDB search returned {igdbGames.Count()} games");
                foreach (var igdbGame in igdbGames)
                {
                    System.Console.WriteLine($"IGDB game: {igdbGame.Name}, ID: {igdbGame.Id}, Cover: {(igdbGame.Cover != null ? "exists" : "null")}, ImageId: {igdbGame.Cover}");

                    // Check if already in local database
                    var existingGame = await _context.Games
                        .FirstOrDefaultAsync(g => g.IgdbId == igdbGame.Id);

                    var criticScore = igdbGame.aggregated_rating.HasValue
                        ? (int?)Math.Round(igdbGame.aggregated_rating.Value)
                        : null;

                    if (existingGame == null)
                    {
                        // Add to local database
                        var newGame = new Game
                        {
                            Id = Guid.NewGuid(),
                            IgdbId = igdbGame.Id,
                            Name = igdbGame.Name,
                            Summary = igdbGame.Summary ?? string.Empty,
                            ReleaseDate = igdbGame.first_release_date.HasValue
                                ? DateTimeOffset.FromUnixTimeSeconds(igdbGame.first_release_date.Value).DateTime
                                : null,
                            CoverUrl = igdbGame.Cover != null && !string.IsNullOrWhiteSpace(igdbGame.Cover.image_id)
                                ? $"https://images.igdb.com/igdb/image/upload/t_cover_big/{igdbGame?.Cover.image_id}.jpg"
                                : null,
                            CriticScore = criticScore,
                            CriticScoreChecked = criticScore.HasValue,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Games.Add(newGame);
                        await _context.SaveChangesAsync();

                        localGames.Add(new GameSummaryDto
                        {
                            Id = newGame.Id,
                            IgdbId = newGame.IgdbId,
                            SteamAppId = newGame.SteamAppId,
                            Name = newGame.Name,
                            ReleaseDate = newGame.ReleaseDate,
                            CoverUrl = newGame.CoverUrl,
                            CriticScore = newGame.CriticScore
                        });
                    }
                    else
                    {
                        // Update CriticScore if currently null
                        if (existingGame.CriticScore == null && criticScore.HasValue)
                        {
                            existingGame.CriticScore = criticScore;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return localGames;
        }

        public async Task<GameDto?> GetGameAsync(Guid id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return null;

            // Lazy-fetch score and/or description from Steam on first page load
            var needsScore = game.CriticScore == null && !game.CriticScoreChecked;
            var needsSummary = string.IsNullOrEmpty(game.Summary) && !game.SummaryFetched;
            if (game.SteamAppId.HasValue && (needsScore || needsSummary))
            {
                var (score, description) = await _steamService.GetSteamAppDetailsAsync(game.SteamAppId.Value);
                if (needsScore)
                {
                    game.CriticScoreChecked = true;
                    if (score.HasValue) game.CriticScore = score;
                }
                if (needsSummary)
                {
                    game.SummaryFetched = true;
                    if (!string.IsNullOrEmpty(description)) game.Summary = description;
                }
                await _context.SaveChangesAsync();
            }

            // Lazy-fetch IGDB ID for Steam-imported games (enables Twitch stream lookup)
            if (game.SteamAppId.HasValue && game.IgdbId == null && !game.IgdbIdChecked)
            {
                game.IgdbIdChecked = true;
                var igdbId = await _igdbService.FindIgdbIdBySteamIdAsync(game.SteamAppId.Value);
                if (igdbId.HasValue) game.IgdbId = igdbId;
                await _context.SaveChangesAsync();
            }

            return new GameDto
            {
                Id = game.Id,
                IgdbId = game.IgdbId,
                SteamAppId = game.SteamAppId,
                Name = game.Name,
                Summary = game.Summary,
                ReleaseDate = game.ReleaseDate,
                CoverUrl = game.CoverUrl,
                CriticScore = game.CriticScore
            };
        }

        public async Task<IEnumerable<RetroArchMatchResultDto>> MatchRetroArchGamesAsync(IEnumerable<RetroArchEntryDto> entries)
        {
            var entryList = entries.ToList();
            if (!entryList.Any()) return Enumerable.Empty<RetroArchMatchResultDto>();

            var uniqueNames = entryList
                .Select(e => e.Name.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Load any already-known games from local DB by name.
            // Use Contains() on a pre-lowercased list — EF Core translates this to a SQL IN clause.
            var lowerNames = uniqueNames.Select(n => n.ToLower()).ToList();
            var localGamesList = await _context.Games
                .Where(g => lowerNames.Contains(g.Name.ToLower()))
                .ToListAsync();

            // Build dictionary, last-write-wins to handle any accidental duplicates in the DB
            var localGames = new Dictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in localGamesList)
                localGames[g.Name.ToLower()] = g;

            // Find names not yet in local DB
            var missingNames = uniqueNames
                .Where(n => !localGames.ContainsKey(n.ToLower()))
                .ToList();

            if (missingNames.Any())
            {
                // Dictionary keyed by the original input name — multiquery maps each result back by index
                var igdbByInputName = await _igdbService.BatchSearchGamesAsync(missingNames);

                // Track games added to the context but not yet saved, keyed by IGDB ID.
                // Needed because multiple input names can resolve to the same IGDB game (e.g. via
                // prefix/suffix fallback), and FirstOrDefaultAsync only queries the DB, not pending inserts.
                var pendingByIgdbId = new Dictionary<long, Game>();

                foreach (var name in missingNames)
                {
                    if (!igdbByInputName.TryGetValue(name, out var igdbGame)) continue;

                    // Check pending (unsaved) inserts first to avoid UNIQUE constraint on IgdbId
                    if (pendingByIgdbId.TryGetValue(igdbGame.Id, out var pendingGame))
                    {
                        localGames[name.ToLower()] = pendingGame;
                        continue;
                    }

                    // Check if this IGDB ID is already in DB under a different name variant
                    var existing = await _context.Games.FirstOrDefaultAsync(g => g.IgdbId == igdbGame.Id);
                    if (existing != null)
                    {
                        localGames[name.ToLower()] = existing;
                        continue;
                    }

                    var criticScore = igdbGame.aggregated_rating.HasValue
                        ? (int?)Math.Round(igdbGame.aggregated_rating.Value)
                        : null;

                    var newGame = new Game
                    {
                        Id = Guid.NewGuid(),
                        IgdbId = igdbGame.Id,
                        Name = igdbGame.Name,
                        Summary = igdbGame.Summary ?? string.Empty,
                        ReleaseDate = igdbGame.first_release_date.HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(igdbGame.first_release_date.Value).DateTime
                            : null,
                        CoverUrl = igdbGame.Cover != null && !string.IsNullOrWhiteSpace(igdbGame.Cover.image_id)
                            ? $"https://images.igdb.com/igdb/image/upload/t_cover_big/{igdbGame.Cover.image_id}.jpg"
                            : null,
                        CriticScore = criticScore,
                        CriticScoreChecked = criticScore.HasValue,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Games.Add(newGame);
                    localGames[name.ToLower()] = newGame;
                    pendingByIgdbId[igdbGame.Id] = newGame;
                }

                await _context.SaveChangesAsync();
            }

            return entryList.Select(entry =>
            {
                var key = entry.Name.Trim().ToLower();
                localGames.TryGetValue(key, out var game);

                return new RetroArchMatchResultDto
                {
                    InputName = entry.Name,
                    Platform = entry.Platform,
                    Game = game == null ? null : new GameSummaryDto
                    {
                        Id = game.Id,
                        IgdbId = game.IgdbId,
                        SteamAppId = game.SteamAppId,
                        Name = game.Name,
                        ReleaseDate = game.ReleaseDate,
                        CoverUrl = game.CoverUrl,
                        CriticScore = game.CriticScore
                    }
                };
            });
        }

        public async Task<GameDto> GetOrFetchGameFromIgdbAsync(long igdbId)
        {
            // Check if game exists in local database
            var game = await _context.Games
                .FirstOrDefaultAsync(g => g.IgdbId == igdbId);

            if (game != null)
            {
                return new GameDto
                {
                    Id = game.Id,
                    IgdbId = game.IgdbId,
                    SteamAppId = game.SteamAppId,
                    Name = game.Name,
                    Summary = game.Summary,
                    ReleaseDate = game.ReleaseDate,
                    CoverUrl = game.CoverUrl,
                    CriticScore = game.CriticScore
                };
            }

            // Fetch from IGDB
            var igdbGame = await _igdbService.GetGameAsync(igdbId);
            if (igdbGame == null)
            {
                throw new ArgumentException($"Game with IGDB ID {igdbId} not found");
            }

            // Create new game in local database
            game = new Game
            {
                Id = Guid.NewGuid(),
                IgdbId = igdbGame.Id,
                Name = igdbGame.Name,
                Summary = igdbGame.Summary ?? string.Empty,
                ReleaseDate = igdbGame.first_release_date.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(igdbGame.first_release_date.Value).DateTime
                    : null,
                CoverUrl = igdbGame.Cover != null && !string.IsNullOrWhiteSpace(igdbGame.Cover.image_id)
                    ? $"https://images.igdb.com/igdb/image/upload/t_cover_big/{igdbGame.Cover.image_id}.jpg"
                    : null,
                CriticScore = igdbGame.aggregated_rating.HasValue
                    ? (int?)Math.Round(igdbGame.aggregated_rating.Value)
                    : null,
                CriticScoreChecked = igdbGame.aggregated_rating.HasValue,
                CreatedAt = DateTime.UtcNow
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return new GameDto
            {
                Id = game.Id,
                IgdbId = game.IgdbId,
                SteamAppId = game.SteamAppId,
                Name = game.Name,
                Summary = game.Summary,
                ReleaseDate = game.ReleaseDate,
                CoverUrl = game.CoverUrl,
                CriticScore = game.CriticScore
            };
        }
    }
}