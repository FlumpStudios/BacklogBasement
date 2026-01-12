using System;
using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class SteamImportRequest
    {
        public bool IncludePlaytime { get; set; }
    }

    public class SteamImportResult
    {
        public int TotalGames { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public List<SteamImportedGameDto> ImportedGames { get; set; } = new();
        public List<SteamSkippedGameDto> SkippedGames { get; set; } = new();
        public List<SteamFailedGameDto> FailedGames { get; set; } = new();
    }

    public class SteamImportedGameDto
    {
        public Guid GameId { get; set; }
        public string Name { get; set; } = string.Empty;
        public long SteamAppId { get; set; }
        public long? IgdbId { get; set; }
        public bool MatchedToIgdb { get; set; }
        public int? PlaytimeMinutes { get; set; }
    }

    public class SteamSkippedGameDto
    {
        public string Name { get; set; } = string.Empty;
        public long SteamAppId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class SteamFailedGameDto
    {
        public string Name { get; set; } = string.Empty;
        public long SteamAppId { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class SteamStatusDto
    {
        public bool IsLinked { get; set; }
        public string? SteamId { get; set; }
    }
}
