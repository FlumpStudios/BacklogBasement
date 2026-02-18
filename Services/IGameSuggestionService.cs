using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IGameSuggestionService
    {
        Task<GameSuggestionDto> SendSuggestionAsync(Guid senderUserId, SendGameSuggestionRequest request);
        Task<List<GameSuggestionDto>> GetReceivedSuggestionsAsync(Guid userId);
        Task DismissSuggestionAsync(Guid userId, Guid suggestionId);
    }
}
