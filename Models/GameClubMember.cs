using System;

namespace BacklogBasement.Models
{
    public class GameClubMember
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "member"; // "owner" | "admin" | "member"
        public DateTime JoinedAt { get; set; }

        // Navigation properties
        public GameClub Club { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
