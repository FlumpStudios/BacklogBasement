using System;

namespace BacklogBasement.Models
{
    public class GamePassword
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public string Password { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string? Notes { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Game Game { get; set; } = null!;
    }
}
