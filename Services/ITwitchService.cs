using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public record TwitchStreamedGame(long IgdbId, string GameName, int TotalMinutes);

    public interface ITwitchService
    {
        Task<IEnumerable<TwitchStreamDto>> GetLiveStreamsForGameAsync(long igdbId, int limit = 6);
        Task<TwitchLiveDto> GetLiveStreamAsync(string twitchUserId);
        Task<List<TwitchStreamedGame>> GetStreamedGamesAsync(string twitchUserId);
    }
}
