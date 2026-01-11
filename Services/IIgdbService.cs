using System.Collections.Generic;
using System.Threading.Tasks;

namespace BacklogBasement.Services
{
    public interface IIgdbService
    {
        Task<IEnumerable<IgdbGame>> SearchGamesAsync(string query);
        Task<IgdbGame?> GetGameAsync(long igdbId);
    }

    public class IgdbGame
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public long? FirstReleaseDate { get; set; }
        public string? Cover { get; set; }
    }

    public class IgdbCover
    {
        public string ImageId { get; set; } = string.Empty;
    }
}