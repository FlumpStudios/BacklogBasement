using System.Collections.Generic;
using System.Threading.Tasks;

namespace BacklogBasement.Services
{
    public interface ISteamService
    {
        Task<IEnumerable<SteamGame>> GetOwnedGamesAsync(string steamId);
        Task<int?> GetGamePlaytimeAsync(string steamId, long steamAppId);
        Task<int?> GetMetacriticScoreAsync(long steamAppId);
        Task<(int? MetacriticScore, string? Description)> GetSteamAppDetailsAsync(long steamAppId);
    }

    public class SteamGame
    {
        public long AppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlaytimeForever { get; set; } // Total playtime in minutes
        public int Playtime2Weeks { get; set; } // Playtime in last 2 weeks in minutes
        public string? ImgIconUrl { get; set; }
        public string? ImgLogoUrl { get; set; }
        public string? HeaderUrl { get; set; } // Steam CDN header image (460x215)
    }

    public class SteamOwnedGamesResponse
    {
        public SteamOwnedGamesInner? Response { get; set; }
    }

    public class SteamOwnedGamesInner
    {
        public int GameCount { get; set; }
        public List<SteamGameRaw>? Games { get; set; }
    }

    public class SteamGameRaw
    {
        public long Appid { get; set; }
        public string? Name { get; set; }
        public int Playtime_Forever { get; set; }
        public int Playtime_2weeks { get; set; }
        public string? Img_Icon_Url { get; set; }
        public string? Img_Logo_Url { get; set; }
    }
}
