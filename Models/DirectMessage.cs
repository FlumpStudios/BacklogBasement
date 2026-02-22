using System;

namespace BacklogBasement.Models
{
    public class DirectMessage
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User Sender { get; set; } = null!;
        public User Recipient { get; set; } = null!;
    }
}
