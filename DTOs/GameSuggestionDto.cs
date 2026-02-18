using System;

namespace BacklogBasement.DTOs
{
    public class GameSuggestionDto
    {
        public Guid Id { get; set; }
        public Guid SenderUserId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendGameSuggestionRequest
    {
        public Guid RecipientUserId { get; set; }
        public Guid GameId { get; set; }
        public string? Message { get; set; }
    }
}
