using System;
using System.Collections.Generic;

namespace BacklogBasement.Models
{
    public class DailyQuiz
    {
        public Guid Id { get; set; }
        public string QuizDate { get; set; } = string.Empty; // "yyyy-MM-dd"
        public string QuestionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<DailyQuizOption> Options { get; set; } = new List<DailyQuizOption>();
        public ICollection<DailyQuizAnswer> Answers { get; set; } = new List<DailyQuizAnswer>();
    }
}
