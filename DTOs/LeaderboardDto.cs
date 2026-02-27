using System;

namespace BacklogBasement.DTOs
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int XpTotal { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public bool IsCurrentUser { get; set; }
    }
}
