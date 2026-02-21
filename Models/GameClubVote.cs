using System;

namespace BacklogBasement.Models
{
    public class GameClubVote
    {
        public Guid Id { get; set; }
        public Guid RoundId { get; set; }
        public Guid UserId { get; set; }
        public Guid NominationId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public GameClubRound Round { get; set; } = null!;
        public User User { get; set; } = null!;
        public GameClubNomination Nomination { get; set; } = null!;
    }
}
