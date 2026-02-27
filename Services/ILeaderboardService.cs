using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(Guid currentUserId, int limit = 100);
        Task<List<LeaderboardEntryDto>> GetFriendLeaderboardAsync(Guid currentUserId);
    }
}
