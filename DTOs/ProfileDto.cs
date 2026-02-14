using System;
using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class ProfileDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
        public ProfileStatsDto Stats { get; set; } = new();
        public List<CollectionItemDto> CurrentlyPlaying { get; set; } = new();
        public List<CollectionItemDto> Backlog { get; set; } = new();
        public List<CollectionItemDto> Collection { get; set; } = new();
        public List<FriendDto> Friends { get; set; } = new();
    }

    public class ProfileStatsDto
    {
        public int TotalGames { get; set; }
        public int TotalPlayTimeMinutes { get; set; }
        public int BacklogCount { get; set; }
        public int PlayingCount { get; set; }
        public int CompletedCount { get; set; }
        public int FriendCount { get; set; }
    }

    public class SetUsernameRequest
    {
        public string Username { get; set; } = string.Empty;
    }

    public class UsernameAvailabilityResponse
    {
        public bool Available { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
