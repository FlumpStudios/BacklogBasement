using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class DailyPoll
    {
        public Guid Id { get; set; }
        public string PollDate { get; set; } = string.Empty; // "yyyy-MM-dd"
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<DailyPollGame> Games { get; set; } = new List<DailyPollGame>();
        public ICollection<DailyPollVote> Votes { get; set; } = new List<DailyPollVote>();
    }
}
