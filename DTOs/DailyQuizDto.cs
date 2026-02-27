using System;
using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class DailyQuizOptionDto
    {
        public Guid OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
    }

    public class DailyQuizResultDto
    {
        public Guid OptionId { get; set; }
        public int AnswerCount { get; set; }
        public double Percentage { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class DailyQuizDto
    {
        public Guid QuizId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public List<DailyQuizOptionDto> Options { get; set; } = new();
        public Guid? UserSelectedOptionId { get; set; }
        public bool? UserWasCorrect { get; set; }
        public List<DailyQuizResultDto>? Results { get; set; }
    }

    public class AnswerQuizRequest
    {
        public Guid OptionId { get; set; }
    }
}
