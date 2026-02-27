using System;
using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class DailyPollGameDto
    {
        public Guid GameId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
    }

    public class PollResultDto
    {
        public Guid GameId { get; set; }
        public int VoteCount { get; set; }
        public double Percentage { get; set; }
    }

    public class DailyPollDto
    {
        public Guid PollId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<DailyPollGameDto> Games { get; set; } = new();
        public Guid? UserVotedGameId { get; set; }
        public List<PollResultDto>? Results { get; set; }
    }

    public class VotePollRequest
    {
        public Guid GameId { get; set; }
    }
}
