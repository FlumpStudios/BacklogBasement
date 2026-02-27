using System;

namespace BacklogBasement.Models
{
    public class DailyPollGame
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public Guid GameId { get; set; }

        public DailyPoll Poll { get; set; } = null!;
        public Game Game { get; set; } = null!;
    }
}
