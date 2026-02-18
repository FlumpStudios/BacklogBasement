using System;

namespace BacklogBasement.Models
{
    public class GameSuggestion
    {
        public Guid Id { get; set; }
        public Guid SenderUserId { get; set; }
        public Guid RecipientUserId { get; set; }
        public Guid GameId { get; set; }
        public string? Message { get; set; }
        public bool IsDismissed { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User Sender { get; set; } = null!;
        public User Recipient { get; set; } = null!;
        public Game Game { get; set; } = null!;
    }
}
