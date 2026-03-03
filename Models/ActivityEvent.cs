using System;

namespace BacklogBasement.Models
{
    public class ActivityEvent
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string EventType { get; set; } = string.Empty;
        public Guid? GameId { get; set; }
        public Game? Game { get; set; }
        public Guid? ClubId { get; set; }
        public GameClub? Club { get; set; }
        public int? IntValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
