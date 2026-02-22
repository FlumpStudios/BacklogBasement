using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class GameClub
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public string? DiscordLink { get; set; }
        public string? WhatsAppLink { get; set; }
        public string? RedditLink { get; set; }
        public string? YouTubeLink { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User Owner { get; set; } = null!;
        public ICollection<GameClubMember> Members { get; set; } = new List<GameClubMember>();
        public ICollection<GameClubRound> Rounds { get; set; } = new List<GameClubRound>();
        public ICollection<GameClubInvite> Invites { get; set; } = new List<GameClubInvite>();
    }
}
