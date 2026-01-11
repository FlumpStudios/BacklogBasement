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

        public CollectionService(ApplicationDbContext context, IPlaySessionService playSessionService)
        {
            _context = context;
            _playSessionService = playSessionService;
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
                    TotalPlayTimeMinutes = 0 // Will be populated separately
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
                TotalPlayTimeMinutes = 0
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
                TotalPlayTimeMinutes = totalPlayTime
            };
        }
    }
}