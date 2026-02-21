using System;

namespace BacklogBasement.Models
{
    public class GameClubInvite
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public Guid InvitedByUserId { get; set; }
        public Guid InviteeUserId { get; set; }
        public string Status { get; set; } = "pending"; // "pending" | "accepted" | "declined"
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        // Navigation properties
        public GameClub Club { get; set; } = null!;
        public User InvitedByUser { get; set; } = null!;
        public User Invitee { get; set; } = null!;
    }
}
