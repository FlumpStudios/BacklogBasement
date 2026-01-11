using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IGameService
    {
        Task<IEnumerable<GameSummaryDto>> SearchGamesAsync(string query);
        Task<GameDto?> GetGameAsync(Guid id);
        Task<GameDto> GetOrFetchGameFromIgdbAsync(long igdbId);
    }
}