using System;

namespace BacklogBasement.Models
{
    public class GameClubReview
    {
        public Guid Id { get; set; }
        public Guid RoundId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime SubmittedAt { get; set; }

        // Navigation properties
        public GameClubRound Round { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
