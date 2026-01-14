using System;

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
    }

    public class AddToCollectionRequest
    {
        public Guid GameId { get; set; }
        public string? Notes { get; set; }
    }
}