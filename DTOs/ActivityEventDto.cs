using System;

namespace BacklogBasement.DTOs
{
    public class ActivityEventDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public Guid? GameId { get; set; }
        public string? GameName { get; set; }
        public string? GameCoverUrl { get; set; }
        public Guid? ClubId { get; set; }
        public string? ClubName { get; set; }
        public int? IntValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
