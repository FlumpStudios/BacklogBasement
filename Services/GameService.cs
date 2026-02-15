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

            // Lazy-fetch Metacritic score from Steam if we don't have a score and haven't checked yet
            if (game.CriticScore == null && !game.CriticScoreChecked && game.SteamAppId.HasValue)
            {
                var score = await _steamService.GetMetacriticScoreAsync(game.SteamAppId.Value);
                game.CriticScoreChecked = true;
                if (score.HasValue)
                    game.CriticScore = score;
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