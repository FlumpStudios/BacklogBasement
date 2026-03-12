using System;

namespace BacklogBasement.Models
{
    public class FeaturedGame
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public DateTime AddedAt { get; set; }

        // Navigation properties
        public Game Game { get; set; } = null!;
    }
}
