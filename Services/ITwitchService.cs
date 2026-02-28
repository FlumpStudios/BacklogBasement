using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface ITwitchService
    {
        Task<IEnumerable<TwitchStreamDto>> GetLiveStreamsForGameAsync(long igdbId, int limit = 6);
    }
}
