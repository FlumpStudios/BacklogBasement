using System;

namespace BacklogBasement.DTOs
{
    public class ConversationDto
    {
        public Guid FriendUserId { get; set; }
        public string FriendUsername { get; set; } = string.Empty;
        public string FriendDisplayName { get; set; } = string.Empty;
        public string LastMessageContent { get; set; } = string.Empty;
        public bool LastMessageIsFromMe { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class DirectMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderDisplayName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
