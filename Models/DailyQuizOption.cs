using System;

namespace BacklogBasement.Models
{
    public class DailyQuizOption
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? CoverUrl { get; set; }
        public int DisplayOrder { get; set; }

        public DailyQuiz Quiz { get; set; } = null!;
    }
}
