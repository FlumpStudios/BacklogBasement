using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BacklogBasement.Services
{
    public class MockIgdbService : IIgdbService
    {
        private readonly List<IgdbGame> _mockGames = new()
        {
            new IgdbGame
            {
                Id = 289497, // Super Mario Bros
                Name = "Super Mario Bros.",
                Summary = "A side-scrolling platformer game where the player controls Mario (or Luigi) as he traverses the Mushroom Kingdom. The game consists of eight worlds with four sub-levels in each world.",
                first_release_date = 499161600, // 1985-09-13
                // Cover = new IgdbCover { ImageId = "co1sf8" }
            },
            new IgdbGame
            {
                Id = 2902, // The Legend of Zelda
                Name = "The Legend of Zelda",
                Summary = "Set in the fantasy world of Hyrule, the plot centers on Link, a young Hylian boy, who must rescue Princess Zelda from the villain Ganon.",
                first_release_date = 529785600, // 1986-09-26
                // Cover = new IgdbCover { ImageId = "co1rgi" }
            },
            new IgdbGame
            {
                Id = 289738, // Metroid
                Name = "Metroid",
                Summary = "A science fiction action-adventure game set on the planet Zebes. The player controls Samus Aran, a bounty hunter who must explore the planet and defeat the Space Pirates.",
                first_release_date = 548179200, // 1987-08-06
                // Cover = new IgdbCover { ImageId = "co1r7h" }
            }
        };

        public Task<IEnumerable<IgdbGame>> SearchGamesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Task.FromResult<IEnumerable<IgdbGame>>(Array.Empty<IgdbGame>());

            var results = _mockGames
                .Where(g => g.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult<IEnumerable<IgdbGame>>(results);
        }

        public Task<IgdbGame?> GetGameAsync(long igdbId)
        {
            var game = _mockGames.FirstOrDefault(g => g.Id == igdbId);
            return Task.FromResult(game);
        }

        public Task<long?> FindIgdbIdBySteamIdAsync(long steamAppId)
            => Task.FromResult<long?>(null);

        public Task<Dictionary<string, IgdbGame>> BatchSearchGamesAsync(IEnumerable<string> names)
        {
            var result = new Dictionary<string, IgdbGame>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                var game = _mockGames.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (game != null) result[name] = game;
            }
            return Task.FromResult(result);
        }
    }
}