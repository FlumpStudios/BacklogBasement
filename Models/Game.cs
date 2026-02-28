using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public long? IgdbId { get; set; }
        public long? SteamAppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? CoverUrl { get; set; }
        public int? CriticScore { get; set; }
        public bool CriticScoreChecked { get; set; }
        public bool SummaryFetched { get; set; }
        public bool IgdbIdChecked { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
        public ICollection<GameSuggestion> GameSuggestions { get; set; } = new List<GameSuggestion>();
        public ICollection<GameClubNomination> GameClubNominations { get; set; } = new List<GameClubNomination>();
        public ICollection<GameClubRound> GameClubRounds { get; set; } = new List<GameClubRound>();
    }
}