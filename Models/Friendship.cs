using System;

namespace BacklogBasement.Models
{
    public class Friendship
    {
        public Guid Id { get; set; }
        public Guid RequesterId { get; set; }
        public Guid AddresseeId { get; set; }
        public string Status { get; set; } = "pending"; // "pending", "accepted", "declined"
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        // Navigation properties
        public User Requester { get; set; } = null!;
        public User Addressee { get; set; } = null!;
    }
}
