using System;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IDailyPollService
    {
        Task<DailyPollDto> GetOrCreateTodaysPollAsync(Guid userId);
        Task<DailyPollDto?> GetPreviousPollAsync(Guid userId);
        Task<DailyPollDto> VoteAsync(Guid userId, Guid pollId, Guid gameId);
    }
}
