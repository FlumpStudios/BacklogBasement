using System;

namespace BacklogBasement.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid? RelatedUserId { get; set; }
        public string? RelatedUsername { get; set; }
        public Guid? RelatedGameId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UnreadCountDto
    {
        public int Count { get; set; }
    }
}
