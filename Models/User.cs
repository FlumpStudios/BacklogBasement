using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? GoogleSubjectId { get; set; }
        public string? SteamId { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int XpTotal { get; set; }

        // Navigation properties
        public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
        public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<GameSuggestion> SentSuggestions { get; set; } = new List<GameSuggestion>();
        public ICollection<GameSuggestion> ReceivedSuggestions { get; set; } = new List<GameSuggestion>();
        public ICollection<GameClubMember> GameClubMemberships { get; set; } = new List<GameClubMember>();
        public ICollection<GameClubInvite> SentClubInvites { get; set; } = new List<GameClubInvite>();
        public ICollection<GameClubInvite> ReceivedClubInvites { get; set; } = new List<GameClubInvite>();
        public ICollection<DirectMessage> SentMessages { get; set; } = new List<DirectMessage>();
        public ICollection<DirectMessage> ReceivedMessages { get; set; } = new List<DirectMessage>();
        public ICollection<DailyPollVote> PollVotes { get; set; } = new List<DailyPollVote>();
    }
}