using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IFeaturedGameService
    {
        Task<List<GameDto>> GetFeaturedAsync();
        Task AddFeaturedAsync(Guid gameId);
        Task RemoveFeaturedAsync(Guid gameId);
        Task<List<GameDto>> SearchGamesInDbAsync(string query);
    }
}
