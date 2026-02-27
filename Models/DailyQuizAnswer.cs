using System;

namespace BacklogBasement.Models
{
    public class DailyQuizAnswer
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public Guid UserId { get; set; }
        public Guid SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime CreatedAt { get; set; }

        public DailyQuiz Quiz { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
