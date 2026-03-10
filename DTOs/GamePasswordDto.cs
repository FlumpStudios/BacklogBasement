using System;
using System.ComponentModel.DataAnnotations;

namespace BacklogBasement.DTOs
{
    public class GamePasswordDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string Password { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string? Notes { get; set; }
        public bool IsPublic { get; set; }
        public string? SubmittedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateGamePasswordRequest
    {
        [Required]
        [MaxLength(500)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Label { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsPublic { get; set; }
    }
}
