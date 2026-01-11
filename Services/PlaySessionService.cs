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
    public class PlaySessionService : IPlaySessionService
    {
        private readonly ApplicationDbContext _context;

        public PlaySessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PlaySessionDto?> AddPlaySessionAsync(Guid userId, Guid gameId, AddPlaySessionRequest request)
        {
            // Validate request
            if (request.DurationMinutes <= 0)
            {
                throw new BadRequestException("Duration must be greater than 0");
            }

            if (request.PlayedAt > DateTime.UtcNow)
            {
                throw new BadRequestException("PlayedAt date cannot be in the future");
            }

            // Check if game is in user's collection
            var userGame = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame == null)
            {
                throw new NotFoundException($"Game with ID {gameId} is not in your collection");
            }

            // Create play session
            var playSession = new PlaySession
            {
                Id = Guid.NewGuid(),
                UserGameId = userGame.Id,
                DurationMinutes = request.DurationMinutes,
                PlayedAt = request.PlayedAt
            };

            _context.PlaySessions.Add(playSession);
            await _context.SaveChangesAsync();

            return new PlaySessionDto
            {
                Id = playSession.Id,
                UserGameId = playSession.UserGameId,
                DurationMinutes = playSession.DurationMinutes,
                PlayedAt = playSession.PlayedAt
            };
        }

        public async Task<IEnumerable<PlaySessionDto>> GetPlaySessionsAsync(Guid userId, Guid gameId)
        {
            return await _context.PlaySessions
                .Include(ps => ps.UserGame)
                .Where(ps => ps.UserGame.UserId == userId && ps.UserGame.GameId == gameId)
                .OrderByDescending(ps => ps.PlayedAt)
                .Select(ps => new PlaySessionDto
                {
                    Id = ps.Id,
                    UserGameId = ps.UserGameId,
                    DurationMinutes = ps.DurationMinutes,
                    PlayedAt = ps.PlayedAt
                })
                .ToListAsync();
        }

        public async Task<bool> DeletePlaySessionAsync(Guid userId, Guid playSessionId)
        {
            // Find the play session and verify it belongs to the user
            var playSession = await _context.PlaySessions
                .Include(ps => ps.UserGame)
                .FirstOrDefaultAsync(ps => ps.Id == playSessionId && ps.UserGame.UserId == userId);

            if (playSession == null)
            {
                return false;
            }

            _context.PlaySessions.Remove(playSession);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalPlayTimeAsync(Guid userId, Guid gameId)
        {
            return await _context.PlaySessions
                .Include(ps => ps.UserGame)
                .Where(ps => ps.UserGame.UserId == userId && ps.UserGame.GameId == gameId)
                .SumAsync(ps => ps.DurationMinutes);
        }
    }
}