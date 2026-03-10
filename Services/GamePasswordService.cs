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
    public class GamePasswordService : IGamePasswordService
    {
        private readonly ApplicationDbContext _db;
        private readonly IProfanityService _profanity;

        public GamePasswordService(ApplicationDbContext db, IProfanityService profanity)
        {
            _db = db;
            _profanity = profanity;
        }

        public async Task<List<GamePasswordDto>> GetPasswordsAsync(Guid userId, Guid gameId)
        {
            return await _db.GamePasswords
                .Where(p => p.UserId == userId && p.GameId == gameId)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new GamePasswordDto
                {
                    Id = p.Id,
                    GameId = p.GameId,
                    Password = p.Password,
                    Label = p.Label,
                    Notes = p.Notes,
                    IsPublic = p.IsPublic,
                    SubmittedBy = null,
                    CreatedAt = p.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task<List<GamePasswordDto>> GetPublicPasswordsAsync(Guid gameId)
        {
            return await _db.GamePasswords
                .Where(p => p.GameId == gameId && p.IsPublic)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new GamePasswordDto
                {
                    Id = p.Id,
                    GameId = p.GameId,
                    Password = p.Password,
                    Label = p.Label,
                    Notes = p.Notes,
                    IsPublic = true,
                    SubmittedBy = p.User.DisplayName,
                    CreatedAt = p.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task<GamePasswordDto> AddPasswordAsync(Guid userId, Guid gameId, CreateGamePasswordRequest request)
        {
            var gameExists = await _db.Games.AnyAsync(g => g.Id == gameId);
            if (!gameExists)
                throw new NotFoundException("Game not found");

            if (request.IsPublic)
            {
                _profanity.AssertClean(request.Password, "Password");
                _profanity.AssertClean(request.Label, "Label");
                _profanity.AssertClean(request.Notes, "Notes");
            }

            var password = new GamePassword
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = gameId,
                Password = request.Password.Trim(),
                Label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim(),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow,
            };

            _db.GamePasswords.Add(password);
            await _db.SaveChangesAsync();

            return new GamePasswordDto
            {
                Id = password.Id,
                GameId = password.GameId,
                Password = password.Password,
                Label = password.Label,
                Notes = password.Notes,
                IsPublic = password.IsPublic,
                SubmittedBy = null,
                CreatedAt = password.CreatedAt,
            };
        }

        public async Task DeletePasswordAsync(Guid userId, Guid passwordId)
        {
            var password = await _db.GamePasswords
                .FirstOrDefaultAsync(p => p.Id == passwordId && p.UserId == userId);

            if (password == null)
                throw new NotFoundException("Password not found");

            _db.GamePasswords.Remove(password);
            await _db.SaveChangesAsync();
        }
    }
}
