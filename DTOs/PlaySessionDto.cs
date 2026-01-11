using System;

namespace BacklogBasement.DTOs
{
    public class PlaySessionDto
    {
        public Guid Id { get; set; }
        public Guid UserGameId { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime PlayedAt { get; set; }
    }

    public class AddPlaySessionRequest
    {
        public int DurationMinutes { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}