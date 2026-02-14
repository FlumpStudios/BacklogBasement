using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string GoogleSubjectId { get; set; } = string.Empty;
        public string? SteamId { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
        public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}