using System;
using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class FriendDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime FriendsSince { get; set; }
    }

    public class FriendRequestDto
    {
        public Guid FriendshipId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string Direction { get; set; } = string.Empty; // "incoming" or "outgoing"
    }

    public class FriendshipStatusDto
    {
        public string Status { get; set; } = "none"; // "none", "pending_outgoing", "pending_incoming", "friends"
        public Guid? FriendshipId { get; set; }
    }

    public class PlayerSearchResultDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
    }
}
