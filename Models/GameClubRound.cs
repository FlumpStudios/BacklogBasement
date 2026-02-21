using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class GameClubRound
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public int RoundNumber { get; set; }
        public Guid? GameId { get; set; }
        public string Status { get; set; } = "nominating"; // "nominating" | "voting" | "playing" | "reviewing" | "completed"
        public DateTime? NominatingDeadline { get; set; }
        public DateTime? VotingDeadline { get; set; }
        public DateTime? PlayingDeadline { get; set; }
        public DateTime? ReviewingDeadline { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public GameClub Club { get; set; } = null!;
        public Game? Game { get; set; }
        public ICollection<GameClubNomination> Nominations { get; set; } = new List<GameClubNomination>();
        public ICollection<GameClubVote> Votes { get; set; } = new List<GameClubVote>();
        public ICollection<GameClubReview> Reviews { get; set; } = new List<GameClubReview>();
    }
}
