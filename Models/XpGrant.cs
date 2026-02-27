using System;

namespace BacklogBasement.Models
{
    public class XpGrant
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        public int XpAwarded { get; set; }
        public DateTime GrantedAt { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
