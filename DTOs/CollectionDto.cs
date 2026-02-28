using System;
using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class CollectionItemDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime DateAdded { get; set; }
        public string? Notes { get; set; }
        public int TotalPlayTimeMinutes { get; set; }
        public string Source { get; set; } = "manual"; // "steam" or "manual"
        public string? Status { get; set; } // null, "backlog", "playing", "completed"
        public DateTime? DateCompleted { get; set; }
        public int? CriticScore { get; set; }
    }

    public class AddToCollectionRequest
    {
        public Guid GameId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateGameStatusRequest
    {
        public string? Status { get; set; } // null, "backlog", "playing", "completed"
    }

    public class BulkAddRequest
    {
        public List<Guid> GameIds { get; set; } = new();
    }

    public class PagedCollectionDto
    {
        public List<CollectionItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public bool HasMore { get; set; }
    }

    public class CollectionStatsDto
    {
        public int TotalGames { get; set; }
        public int GamesBacklog { get; set; }
        public int GamesPlaying { get; set; }
        public int GamesCompleted { get; set; }
    }
}