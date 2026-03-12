using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class FeaturedGameService : IFeaturedGameService
    {
        private const int MaxFeatured = 5;
        private readonly ApplicationDbContext _db;

        public FeaturedGameService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<GameDto>> GetFeaturedAsync()
        {
            return await _db.FeaturedGames
                .OrderBy(fg => fg.AddedAt)
                .Select(fg => new GameDto
                {
                    Id = fg.Game.Id,
                    IgdbId = fg.Game.IgdbId,
                    SteamAppId = fg.Game.SteamAppId,
                    Name = fg.Game.Name,
                    Summary = fg.Game.Summary,
                    ReleaseDate = fg.Game.ReleaseDate,
                    CoverUrl = fg.Game.CoverUrl,
                    CriticScore = fg.Game.CriticScore,
                })
                .ToListAsync();
        }

        public async Task AddFeaturedAsync(Guid gameId)
        {
            var count = await _db.FeaturedGames.CountAsync();
            if (count >= MaxFeatured)
                throw new BadRequestException($"Cannot feature more than {MaxFeatured} games. Remove one first.");

            var already = await _db.FeaturedGames.AnyAsync(fg => fg.GameId == gameId);
            if (already)
                throw new BadRequestException("This game is already featured.");

            var gameExists = await _db.Games.AnyAsync(g => g.Id == gameId);
            if (!gameExists)
                throw new NotFoundException("Game not found.");

            _db.FeaturedGames.Add(new FeaturedGame
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                AddedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
        }

        public async Task RemoveFeaturedAsync(Guid gameId)
        {
            var featured = await _db.FeaturedGames.FirstOrDefaultAsync(fg => fg.GameId == gameId);
            if (featured == null)
                throw new NotFoundException("Game is not currently featured.");

            _db.FeaturedGames.Remove(featured);
            await _db.SaveChangesAsync();
        }

        public async Task<List<GameDto>> SearchGamesInDbAsync(string query)
        {
            return await _db.Games
                .Where(g => EF.Functions.Like(g.Name, $"%{query}%"))
                .OrderBy(g => g.Name)
                .Take(20)
                .Select(g => new GameDto
                {
                    Id = g.Id,
                    IgdbId = g.IgdbId,
                    SteamAppId = g.SteamAppId,
                    Name = g.Name,
                    Summary = g.Summary,
                    ReleaseDate = g.ReleaseDate,
                    CoverUrl = g.CoverUrl,
                    CriticScore = g.CriticScore,
                })
                .ToListAsync();
        }
    }
}
