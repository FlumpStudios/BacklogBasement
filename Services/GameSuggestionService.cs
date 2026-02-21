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
    public class GameSuggestionService : IGameSuggestionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFriendshipService _friendshipService;
        private readonly INotificationService _notificationService;
        private readonly IProfanityService _profanityService;

        public GameSuggestionService(
            ApplicationDbContext context,
            IFriendshipService friendshipService,
            INotificationService notificationService,
            IProfanityService profanityService)
        {
            _context = context;
            _friendshipService = friendshipService;
            _notificationService = notificationService;
            _profanityService = profanityService;
        }

        public async Task<GameSuggestionDto> SendSuggestionAsync(Guid senderUserId, SendGameSuggestionRequest request)
        {
            if (senderUserId == request.RecipientUserId)
                throw new BadRequestException("You cannot suggest a game to yourself.");

            _profanityService.AssertClean(request.Message, "Message");

            var friendshipStatus = await _friendshipService.GetFriendshipStatusAsync(senderUserId, request.RecipientUserId);
            if (friendshipStatus.Status != "friends")
                throw new BadRequestException("You can only suggest games to friends.");

            var game = await _context.Games.FindAsync(request.GameId);
            if (game == null)
                throw new NotFoundException("Game not found.");

            var existingActive = await _context.GameSuggestions
                .AnyAsync(gs => gs.SenderUserId == senderUserId
                    && gs.RecipientUserId == request.RecipientUserId
                    && gs.GameId == request.GameId
                    && !gs.IsDismissed);

            if (existingActive)
                throw new BadRequestException("You have already suggested this game to this friend.");

            var sender = await _context.Users.FindAsync(senderUserId);
            if (sender == null)
                throw new NotFoundException("Sender not found.");

            var suggestion = new GameSuggestion
            {
                Id = Guid.NewGuid(),
                SenderUserId = senderUserId,
                RecipientUserId = request.RecipientUserId,
                GameId = request.GameId,
                Message = request.Message,
                IsDismissed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameSuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                request.RecipientUserId,
                "game_suggestion",
                $"{sender.DisplayName} suggested you play \"{game.Name}\"",
                senderUserId,
                game.Id);

            return new GameSuggestionDto
            {
                Id = suggestion.Id,
                SenderUserId = senderUserId,
                SenderUsername = sender.Username ?? string.Empty,
                SenderDisplayName = sender.DisplayName,
                GameId = game.Id,
                GameName = game.Name,
                CoverUrl = game.CoverUrl,
                Message = suggestion.Message,
                CreatedAt = suggestion.CreatedAt
            };
        }

        public async Task<List<GameSuggestionDto>> GetReceivedSuggestionsAsync(Guid userId)
        {
            return await _context.GameSuggestions
                .Where(gs => gs.RecipientUserId == userId && !gs.IsDismissed)
                .OrderByDescending(gs => gs.CreatedAt)
                .Select(gs => new GameSuggestionDto
                {
                    Id = gs.Id,
                    SenderUserId = gs.SenderUserId,
                    SenderUsername = gs.Sender.Username ?? string.Empty,
                    SenderDisplayName = gs.Sender.DisplayName,
                    GameId = gs.GameId,
                    GameName = gs.Game.Name,
                    CoverUrl = gs.Game.CoverUrl,
                    Message = gs.Message,
                    CreatedAt = gs.CreatedAt
                })
                .ToListAsync();
        }

        public async Task DismissSuggestionAsync(Guid userId, Guid suggestionId)
        {
            var suggestion = await _context.GameSuggestions.FindAsync(suggestionId);
            if (suggestion == null)
                throw new NotFoundException("Suggestion not found.");

            if (suggestion.RecipientUserId != userId)
                throw new BadRequestException("You can only dismiss your own suggestions.");

            suggestion.IsDismissed = true;
            await _context.SaveChangesAsync();
        }
    }
}
