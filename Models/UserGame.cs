using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class UserGame
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public DateTime DateAdded { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; } // null, "backlog", "playing", "completed"
        public DateTime? DateCompleted { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Game Game { get; set; } = null!;
        public ICollection<PlaySession> PlaySessions { get; set; } = new List<PlaySession>();
    }
}