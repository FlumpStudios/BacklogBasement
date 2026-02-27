using System;

namespace BacklogBasement.Models
{
    public class DailyPollVote
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public Guid UserId { get; set; }
        public Guid VotedGameId { get; set; }
        public DateTime CreatedAt { get; set; }

        public DailyPoll Poll { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
