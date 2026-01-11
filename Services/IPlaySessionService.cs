using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IPlaySessionService
    {
        Task<PlaySessionDto?> AddPlaySessionAsync(Guid userId, Guid gameId, AddPlaySessionRequest request);
        Task<IEnumerable<PlaySessionDto>> GetPlaySessionsAsync(Guid userId, Guid gameId);
        Task<bool> DeletePlaySessionAsync(Guid userId, Guid playSessionId);
        Task<int> GetTotalPlayTimeAsync(Guid userId, Guid gameId);
    }
}