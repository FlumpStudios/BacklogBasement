using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class GameClubNomination
    {
        public Guid Id { get; set; }
        public Guid RoundId { get; set; }
        public Guid NominatedByUserId { get; set; }
        public Guid GameId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public GameClubRound Round { get; set; } = null!;
        public User NominatedByUser { get; set; } = null!;
        public Game Game { get; set; } = null!;
        public ICollection<GameClubVote> Votes { get; set; } = new List<GameClubVote>();
    }
}
