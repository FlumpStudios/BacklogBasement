using System.Collections.Generic;
using System.Threading.Tasks;

namespace BacklogBasement.Services
{
    public interface IIgdbService
    {
        Task<IEnumerable<IgdbGame>> SearchGamesAsync(string query);
        Task<IgdbGame?> GetGameAsync(long igdbId);
        Task<Dictionary<string, IgdbGame>> BatchSearchGamesAsync(IEnumerable<string> names);
    }

    public class IgdbGame
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public long? first_release_date { get; set; }
        public double? aggregated_rating { get; set; }
        public IgdbCover? Cover { get; set; }
    }

    public class IgdbCover
    {   
        public string image_id { get; set; } = string.Empty;
    }
}