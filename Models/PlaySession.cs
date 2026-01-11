using System;

namespace BacklogBasement.Models
{
    public class PlaySession
    {
        public Guid Id { get; set; }
        public Guid UserGameId { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime PlayedAt { get; set; }

        // Navigation property
        public UserGame UserGame { get; set; } = null!;
    }
}