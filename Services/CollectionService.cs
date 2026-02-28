using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Models;
using BacklogBasement.Exceptions;

namespace BacklogBasement.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlaySessionService _playSessionService;
        private readonly ISteamService _steamService;

        public CollectionService(ApplicationDbContext context, IPlaySessionService playSessionService, ISteamService steamService)
        {
            _context = context;
            _playSessionService = playSessionService;
            _steamService = steamService;
        }

        public async Task<IEnumerable<CollectionItemDto>> GetUserCollectionAsync(Guid userId)
        {
            return await _context.UserGames
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == userId)
                .OrderByDescending(ug => ug.DateAdded)
                .Select(ug => new CollectionItemDto
                {
                    Id = ug.Id,
                    GameId = ug.GameId,
                    GameName = ug.Game.Name,
                    ReleaseDate = ug.Game.ReleaseDate,
                    CoverUrl = ug.Game.CoverUrl,
                    DateAdded = ug.DateAdded,
                    Notes = ug.Notes,
                    TotalPlayTimeMinutes = 0, // Will be populated separately
                    Source = ug.Game.SteamAppId.HasValue ? "steam" : "manual",
                    Status = ug.Status,
                    DateCompleted = ug.DateCompleted,
                    CriticScore = ug.Game.CriticScore
                })
                .ToListAsync();
        }

        public async Task<CollectionItemDto?> AddGameToCollectionAsync(Guid userId, AddToCollectionRequest request)
        {
            // Check if game exists
            var game = await _context.Games.FindAsync(request.GameId);
            if (game == null)
            {
                throw new NotFoundException($"Game with ID {request.GameId} not found");
            }

            // Check if already in collection
            var existingItem = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == request.GameId);

            if (existingItem != null)
            {
                throw new BadRequestException("Game is already in your collection");
            }

            // Add to collection
            var userGame = new UserGame
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = request.GameId,
                DateAdded = DateTime.UtcNow,
                Notes = request.Notes
            };

            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();

            return new CollectionItemDto
            {
                Id = userGame.Id,
                GameId = userGame.GameId,
                GameName = game.Name,
                ReleaseDate = game.ReleaseDate,
                CoverUrl = game.CoverUrl,
                DateAdded = userGame.DateAdded,
                Notes = userGame.Notes,
                TotalPlayTimeMinutes = 0,
                Source = game.SteamAppId.HasValue ? "steam" : "manual",
                Status = userGame.Status,
                DateCompleted = userGame.DateCompleted,
                CriticScore = game.CriticScore
            };
        }

        public async Task<bool> RemoveGameFromCollectionAsync(Guid userId, Guid gameId)
        {
            var userGame = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame == null)
            {
                return false;
            }

            _context.UserGames.Remove(userGame);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CollectionItemDto?> GetCollectionItemAsync(Guid userId, Guid gameId)
        {
            var userGame = await _context.UserGames
                .Include(ug => ug.Game)
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame == null)
                return null;

            var totalPlayTime = await _playSessionService.GetTotalPlayTimeAsync(userId, gameId);

            return new CollectionItemDto
            {
                Id = userGame.Id,
                GameId = userGame.GameId,
                GameName = userGame.Game.Name,
                ReleaseDate = userGame.Game.ReleaseDate,
                CoverUrl = userGame.Game.CoverUrl,
                DateAdded = userGame.DateAdded,
                Notes = userGame.Notes,
                TotalPlayTimeMinutes = totalPlayTime,
                Source = userGame.Game.SteamAppId.HasValue ? "steam" : "manual",
                Status = userGame.Status,
                DateCompleted = userGame.DateCompleted,
                CriticScore = userGame.Game.CriticScore
            };
        }

        public async Task<CollectionItemDto?> UpdateGameStatusAsync(Guid userId, Guid gameId, string? status)
        {
            // Validate status value
            var validStatuses = new[] { null, "backlog", "playing", "completed" };
            if (status != null && !validStatuses.Contains(status))
            {
                throw new BadRequestException("Invalid status. Must be null, 'backlog', 'playing', or 'completed'");
            }

            var userGame = await _context.UserGames
                .Include(ug => ug.Game)
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame == null)
            {
                throw new NotFoundException("Game not found in collection");
            }

            var previousStatus = userGame.Status;
            userGame.Status = status;

            // Set DateCompleted when marking as completed
            if (status == "completed" && previousStatus != "completed")
            {
                userGame.DateCompleted = DateTime.UtcNow;
            }
            // Clear DateCompleted when moving away from completed
            else if (status != "completed" && previousStatus == "completed")
            {
                userGame.DateCompleted = null;
            }

            // Fetch critic score from Steam if not yet checked
            var game = userGame.Game;
            if (game.CriticScore == null && !game.CriticScoreChecked && game.SteamAppId.HasValue)
            {
                var score = await _steamService.GetMetacriticScoreAsync(game.SteamAppId.Value);
                game.CriticScoreChecked = true;
                if (score.HasValue)
                    game.CriticScore = score;
            }

            await _context.SaveChangesAsync();

            var totalPlayTime = await _playSessionService.GetTotalPlayTimeAsync(userId, gameId);

            return new CollectionItemDto
            {
                Id = userGame.Id,
                GameId = userGame.GameId,
                GameName = userGame.Game.Name,
                ReleaseDate = userGame.Game.ReleaseDate,
                CoverUrl = userGame.Game.CoverUrl,
                DateAdded = userGame.DateAdded,
                Notes = userGame.Notes,
                TotalPlayTimeMinutes = totalPlayTime,
                Source = userGame.Game.SteamAppId.HasValue ? "steam" : "manual",
                Status = userGame.Status,
                DateCompleted = userGame.DateCompleted,
                CriticScore = userGame.Game.CriticScore
            };
        }

        public async Task<(int Added, int AlreadyOwned)> BulkAddGamesAsync(Guid userId, IEnumerable<Guid> gameIds)
        {
            var idList = gameIds.Distinct().ToList();
            if (!idList.Any()) return (0, 0);

            // Load already-owned game IDs in one query
            var ownedIds = await _context.UserGames
                .Where(ug => ug.UserId == userId && idList.Contains(ug.GameId))
                .Select(ug => ug.GameId)
                .ToHashSetAsync();

            // Load valid game IDs in one query
            var validIds = await _context.Games
                .Where(g => idList.Contains(g.Id))
                .Select(g => g.Id)
                .ToHashSetAsync();

            var toAdd = idList
                .Where(id => validIds.Contains(id) && !ownedIds.Contains(id))
                .ToList();

            foreach (var gameId in toAdd)
            {
                _context.UserGames.Add(new UserGame
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    GameId = gameId,
                    DateAdded = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return (toAdd.Count, ownedIds.Count);
        }

        public async Task<CollectionStatsDto> GetCollectionStatsAsync(Guid userId)
        {
            var query = _context.UserGames.Where(ug => ug.UserId == userId);
            return new CollectionStatsDto
            {
                TotalGames = await query.CountAsync(),
                GamesBacklog = await query.CountAsync(ug => ug.Status == "backlog"),
                GamesPlaying = await query.CountAsync(ug => ug.Status == "playing"),
                GamesCompleted = await query.CountAsync(ug => ug.Status == "completed"),
            };
        }

        public async Task<PagedCollectionDto> GetPagedCollectionAsync(
            Guid userId, int skip, int take, string? search, string? status, string? source, string? playStatus, string sortBy, string sortDir)
        {
            var query = _context.UserGames
                .Where(ug => ug.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(ug => ug.Game.Name.ToLower().Contains(search.ToLower()));

            if (status == "none")
                query = query.Where(ug => ug.Status == null);
            else if (!string.IsNullOrWhiteSpace(status) && status != "all")
                query = query.Where(ug => ug.Status == status);

            if (source == "steam")
                query = query.Where(ug => ug.Game.SteamAppId != null);
            else if (source == "manual")
                query = query.Where(ug => ug.Game.SteamAppId == null);

            if (playStatus == "played")
                query = query.Where(ug => ug.PlaySessions.Any());
            else if (playStatus == "unplayed")
                query = query.Where(ug => !ug.PlaySessions.Any());

            var total = await query.CountAsync();

            IOrderedQueryable<UserGame> ordered = (sortBy, sortDir) switch
            {
                ("name", "asc") => query.OrderBy(ug => ug.Game.Name),
                ("name", "desc") => query.OrderByDescending(ug => ug.Game.Name),
                ("release", "desc") => query.OrderByDescending(ug => ug.Game.ReleaseDate),
                ("release", "asc") => query.OrderBy(ug => ug.Game.ReleaseDate),
                ("added", "asc") => query.OrderBy(ug => ug.DateAdded),
                ("playtime", "desc") => query.OrderByDescending(ug => ug.PlaySessions.Sum(ps => ps.DurationMinutes)),
                ("playtime", "asc") => query.OrderBy(ug => ug.PlaySessions.Sum(ps => ps.DurationMinutes)),
                ("score", "desc") => query.OrderByDescending(ug => ug.Game.CriticScore),
                ("score", "asc") => query.OrderBy(ug => ug.Game.CriticScore),
                _ => query.OrderByDescending(ug => ug.DateAdded),
            };

            var items = await ordered
                .Skip(skip)
                .Take(take)
                .Select(ug => new CollectionItemDto
                {
                    Id = ug.Id,
                    GameId = ug.GameId,
                    GameName = ug.Game.Name,
                    ReleaseDate = ug.Game.ReleaseDate,
                    CoverUrl = ug.Game.CoverUrl,
                    DateAdded = ug.DateAdded,
                    Notes = ug.Notes,
                    TotalPlayTimeMinutes = ug.PlaySessions.Sum(ps => ps.DurationMinutes),
                    Source = ug.Game.SteamAppId.HasValue ? "steam" : "manual",
                    Status = ug.Status,
                    DateCompleted = ug.DateCompleted,
                    CriticScore = ug.Game.CriticScore,
                })
                .ToListAsync();

            return new PagedCollectionDto
            {
                Items = items,
                Total = total,
                HasMore = skip + take < total,
            };
        }
    }
}