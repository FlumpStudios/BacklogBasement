using System;

namespace BacklogBasement.DTOs
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public long? IgdbId { get; set; }
        public long? SteamAppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? CoverUrl { get; set; }
        public int? CriticScore { get; set; }
    }

    public class GameSummaryDto
    {
        public Guid Id { get; set; }
        public long? IgdbId { get; set; }
        public long? SteamAppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? CoverUrl { get; set; }
        public int? CriticScore { get; set; }
    }
}