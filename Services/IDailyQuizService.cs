using System;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IDailyQuizService
    {
        Task<DailyQuizDto?> GetOrCreateTodaysQuizAsync(Guid userId);
        Task<DailyQuizDto?> GetPreviousQuizAsync(Guid userId);
        Task<DailyQuizDto> AnswerAsync(Guid userId, Guid quizId, Guid optionId);
    }
}
