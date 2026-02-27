using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class DailyQuizService : IDailyQuizService
    {
        private static readonly string[] QuestionTypes =
        {
            "release_order_first",
            "release_order_last",
            "highest_metacritic",
            "lowest_metacritic",
            "most_collected",
            "most_played",
            "release_year",
            "metacritic_score",
            "true_false_release",
            "true_false_metacritic",
        };

        private readonly ApplicationDbContext _context;

        public DailyQuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DailyQuizDto?> GetOrCreateTodaysQuizAsync(Guid userId)
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var quiz = await LoadQuizAsync(today);
            if (quiz != null)
                return MapToDto(quiz, userId);

            quiz = await CreateQuizAsync(today);
            if (quiz == null)
                return null;

            return MapToDto(quiz, userId);
        }

        public async Task<DailyQuizDto?> GetPreviousQuizAsync(Guid userId)
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var quiz = await _context.DailyQuizzes
                .Include(q => q.Options)
                .Include(q => q.Answers)
                .Where(q => q.QuizDate != today)
                .OrderByDescending(q => q.QuizDate)
                .FirstOrDefaultAsync();

            if (quiz == null) return null;

            return MapToDto(quiz, userId, alwaysShowResults: true);
        }

        public async Task<DailyQuizDto> AnswerAsync(Guid userId, Guid quizId, Guid optionId)
        {
            var quiz = await _context.DailyQuizzes
                .Include(q => q.Options)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new NotFoundException("Quiz not found.");

            var option = quiz.Options.FirstOrDefault(o => o.Id == optionId);
            if (option == null)
                throw new BadRequestException("Option is not part of this quiz.");

            if (quiz.Answers.Any(a => a.UserId == userId))
                throw new BadRequestException("You have already answered this quiz.");

            var answer = new DailyQuizAnswer
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                UserId = userId,
                SelectedOptionId = optionId,
                IsCorrect = option.IsCorrect,
                CreatedAt = DateTime.UtcNow,
            };

            _context.DailyQuizAnswers.Add(answer);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new BadRequestException("You have already answered this quiz.");
            }

            // Reload to get fresh answer counts
            quiz = (await LoadQuizAsync(quiz.QuizDate))!;

            return MapToDto(quiz, userId);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private async Task<DailyQuiz?> LoadQuizAsync(string quizDate)
        {
            return await _context.DailyQuizzes
                .Include(q => q.Options)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuizDate == quizDate);
        }

        private async Task<DailyQuiz?> CreateQuizAsync(string today)
        {
            var seed = int.Parse(today.Replace("-", ""));
            var rng = new Random(seed);

            // Build candidate pool: prefer games in ≥2 collections
            var popularGameIds = await _context.UserGames
                .GroupBy(ug => ug.GameId)
                .Where(g => g.Count() >= 2)
                .Select(g => g.Key)
                .ToListAsync();

            List<Guid> candidateIds = popularGameIds.Count >= 4
                ? popularGameIds
                : await _context.Games.Select(g => g.Id).ToListAsync();

            if (candidateIds.Count < 2)
                return null;

            // Load candidate games with playtime sums
            var games = await _context.Games
                .Where(g => candidateIds.Contains(g.Id))
                .ToListAsync();

            // Playtime per game (sum of all play sessions across all users)
            var playtimeByGameId = await _context.UserGames
                .Where(ug => candidateIds.Contains(ug.GameId))
                .Include(ug => ug.PlaySessions)
                .GroupBy(ug => ug.GameId)
                .Select(g => new { GameId = g.Key, Total = g.SelectMany(ug => ug.PlaySessions).Sum(ps => ps.DurationMinutes) })
                .ToDictionaryAsync(x => x.GameId, x => x.Total);

            // Collection count per game
            var collectionCountByGameId = await _context.UserGames
                .Where(ug => candidateIds.Contains(ug.GameId))
                .GroupBy(ug => ug.GameId)
                .Select(g => new { GameId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GameId, x => x.Count);

            // Shuffle question types deterministically
            var shuffledTypes = QuestionTypes.OrderBy(_ => rng.Next()).ToList();

            foreach (var questionType in shuffledTypes)
            {
                var quiz = TryBuildQuiz(today, questionType, games, collectionCountByGameId, playtimeByGameId, rng);
                if (quiz == null) continue;

                _context.DailyQuizzes.Add(quiz);
                foreach (var opt in quiz.Options)
                    _context.DailyQuizOptions.Add(opt);

                try
                {
                    await _context.SaveChangesAsync();
                    return await LoadQuizAsync(today);
                }
                catch (DbUpdateException)
                {
                    // Race condition — another request already created today's quiz
                    _context.ChangeTracker.Clear();
                    var existing = await LoadQuizAsync(today);
                    if (existing != null) return existing;
                    throw;
                }
            }

            return null; // No question type had enough data
        }

        private static DailyQuiz? TryBuildQuiz(
            string today,
            string questionType,
            List<Game> games,
            Dictionary<Guid, int> collectionCounts,
            Dictionary<Guid, int> playtimes,
            Random rng)
        {
            switch (questionType)
            {
                case "release_order_first":
                case "release_order_last":
                {
                    var eligible = games.Where(g => g.ReleaseDate.HasValue).ToList();
                    if (eligible.Count < 5) return null;
                    var picked = eligible.OrderBy(_ => rng.Next()).Take(5).ToList();
                    var correct = questionType == "release_order_first"
                        ? picked.OrderBy(g => g.ReleaseDate!.Value).First()
                        : picked.OrderByDescending(g => g.ReleaseDate!.Value).First();
                    var question = questionType == "release_order_first"
                        ? "Which of these games was released first?"
                        : "Which of these games was released most recently?";
                    return BuildGameOptionsQuiz(today, questionType, question, picked, correct, rng);
                }

                case "highest_metacritic":
                case "lowest_metacritic":
                {
                    var eligible = games.Where(g => g.CriticScore.HasValue).ToList();
                    if (eligible.Count < 5) return null;
                    var picked = eligible.OrderBy(_ => rng.Next()).Take(5).ToList();
                    var correct = questionType == "highest_metacritic"
                        ? picked.OrderByDescending(g => g.CriticScore!.Value).First()
                        : picked.OrderBy(g => g.CriticScore!.Value).First();
                    var question = questionType == "highest_metacritic"
                        ? "Which of these games has the highest Metacritic score?"
                        : "Which of these games has the lowest Metacritic score?";
                    return BuildGameOptionsQuiz(today, questionType, question, picked, correct, rng);
                }

                case "most_collected":
                {
                    if (games.Count < 5) return null;
                    var picked = games.OrderBy(_ => rng.Next()).Take(5).ToList();
                    var correct = picked.OrderByDescending(g => collectionCounts.GetValueOrDefault(g.Id, 0)).First();
                    return BuildGameOptionsQuiz(today, questionType,
                        "Which of these games is in the most players' collections?",
                        picked, correct, rng);
                }

                case "most_played":
                {
                    var eligible = games.Where(g => playtimes.GetValueOrDefault(g.Id, 0) > 0).ToList();
                    if (eligible.Count < 5) return null;
                    var picked = eligible.OrderBy(_ => rng.Next()).Take(5).ToList();
                    var correct = picked.OrderByDescending(g => playtimes.GetValueOrDefault(g.Id, 0)).First();
                    return BuildGameOptionsQuiz(today, questionType,
                        "Which of these games has the most playtime logged?",
                        picked, correct, rng);
                }

                case "release_year":
                {
                    var eligible = games.Where(g => g.ReleaseDate.HasValue).ToList();
                    if (eligible.Count < 1) return null;
                    var game = eligible[rng.Next(eligible.Count)];
                    var correctYear = game.ReleaseDate!.Value.Year;
                    // Ensure we have 4 unique wrong years different from correct
                    var allWrongs = new List<int>();
                    foreach (var o in new[] { 1, -1, 2, -2, 3, -3, 4, -4, 5, -5 })
                    {
                        var w = correctYear + o;
                        if (w != correctYear && !allWrongs.Contains(w)) allWrongs.Add(w);
                        if (allWrongs.Count >= 4) break;
                    }
                    var options = allWrongs.Select(y => y.ToString()).Prepend(correctYear.ToString()).ToList();
                    var shuffled = options.OrderBy(_ => rng.Next()).ToList();
                    return BuildTextOptionsQuiz(today, questionType,
                        $"What year did {game.Name} release?",
                        shuffled, correctYear.ToString(), null, null);
                }

                case "metacritic_score":
                {
                    var eligible = games.Where(g => g.CriticScore.HasValue).ToList();
                    if (eligible.Count < 1) return null;
                    var game = eligible[rng.Next(eligible.Count)];
                    var correctScore = game.CriticScore!.Value;
                    var wrongOffsets = new[] { 5, 12, 20, 30 };
                    var wrongs = new List<int>();
                    foreach (var offset in wrongOffsets)
                    {
                        var candidate = rng.Next(2) == 0
                            ? Math.Clamp(correctScore + offset, 1, 100)
                            : Math.Clamp(correctScore - offset, 1, 100);
                        if (candidate != correctScore && !wrongs.Contains(candidate))
                            wrongs.Add(candidate);
                        else
                        {
                            // Try the other direction
                            var alt = candidate == Math.Clamp(correctScore + offset, 1, 100)
                                ? Math.Clamp(correctScore - offset, 1, 100)
                                : Math.Clamp(correctScore + offset, 1, 100);
                            if (alt != correctScore && !wrongs.Contains(alt))
                                wrongs.Add(alt);
                        }
                    }
                    while (wrongs.Count < 4)
                    {
                        var extra = Math.Clamp(correctScore + wrongs.Count + 3, 1, 100);
                        if (!wrongs.Contains(extra) && extra != correctScore) wrongs.Add(extra);
                        else wrongs.Add(Math.Clamp(correctScore - wrongs.Count - 3, 1, 100));
                    }
                    wrongs = wrongs.Take(4).ToList();
                    var options = wrongs.Select(s => s.ToString()).Prepend(correctScore.ToString()).ToList();
                    var shuffled = options.OrderBy(_ => rng.Next()).ToList();
                    return BuildTextOptionsQuiz(today, questionType,
                        $"What is {game.Name}'s Metacritic score?",
                        shuffled, correctScore.ToString(), null, null);
                }

                case "true_false_release":
                {
                    var eligible = games.Where(g => g.ReleaseDate.HasValue).ToList();
                    if (eligible.Count < 2) return null;
                    var shuffled = eligible.OrderBy(_ => rng.Next()).Take(2).ToList();
                    var a = shuffled[0];
                    var b = shuffled[1];
                    if (a.ReleaseDate == b.ReleaseDate) return null; // identical dates — skip
                    var truthValue = a.ReleaseDate < b.ReleaseDate;
                    var question = $"True or False: {a.Name} was released before {b.Name}";
                    return BuildTrueFalseQuiz(today, questionType, question, truthValue, rng);
                }

                case "true_false_metacritic":
                {
                    var eligible = games.Where(g => g.CriticScore.HasValue).ToList();
                    if (eligible.Count < 2) return null;
                    var shuffled = eligible.OrderBy(_ => rng.Next()).Take(2).ToList();
                    var a = shuffled[0];
                    var b = shuffled[1];
                    if (a.CriticScore == b.CriticScore) return null;
                    var truthValue = a.CriticScore > b.CriticScore;
                    var question = $"True or False: {a.Name} has a higher Metacritic score than {b.Name}";
                    return BuildTrueFalseQuiz(today, questionType, question, truthValue, rng);
                }

                default:
                    return null;
            }
        }

        private static DailyQuiz BuildGameOptionsQuiz(
            string today, string questionType, string question,
            List<Game> options, Game correct, Random rng)
        {
            var quiz = new DailyQuiz
            {
                Id = Guid.NewGuid(),
                QuizDate = today,
                QuestionType = questionType,
                QuestionText = question,
                CreatedAt = DateTime.UtcNow,
            };

            var shuffled = options.OrderBy(_ => rng.Next()).ToList();
            for (int i = 0; i < shuffled.Count; i++)
            {
                var g = shuffled[i];
                quiz.Options.Add(new DailyQuizOption
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Text = g.Name,
                    IsCorrect = g.Id == correct.Id,
                    CoverUrl = g.CoverUrl,
                    DisplayOrder = i,
                });
            }

            return quiz;
        }

        private static DailyQuiz BuildTextOptionsQuiz(
            string today, string questionType, string question,
            List<string> options, string correctText,
            string? _unused1, string? _unused2)
        {
            var quiz = new DailyQuiz
            {
                Id = Guid.NewGuid(),
                QuizDate = today,
                QuestionType = questionType,
                QuestionText = question,
                CreatedAt = DateTime.UtcNow,
            };

            for (int i = 0; i < options.Count; i++)
            {
                quiz.Options.Add(new DailyQuizOption
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Text = options[i],
                    IsCorrect = options[i] == correctText,
                    CoverUrl = null,
                    DisplayOrder = i,
                });
            }

            return quiz;
        }

        private static DailyQuiz BuildTrueFalseQuiz(
            string today, string questionType, string question, bool truthValue, Random rng)
        {
            var quiz = new DailyQuiz
            {
                Id = Guid.NewGuid(),
                QuizDate = today,
                QuestionType = questionType,
                QuestionText = question,
                CreatedAt = DateTime.UtcNow,
            };

            // Shuffle True/False order
            var trueFirst = rng.Next(2) == 0;
            quiz.Options.Add(new DailyQuizOption
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                Text = "True",
                IsCorrect = truthValue,
                CoverUrl = null,
                DisplayOrder = trueFirst ? 0 : 1,
            });
            quiz.Options.Add(new DailyQuizOption
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                Text = "False",
                IsCorrect = !truthValue,
                CoverUrl = null,
                DisplayOrder = trueFirst ? 1 : 0,
            });

            return quiz;
        }

        private static DailyQuizDto MapToDto(DailyQuiz quiz, Guid userId, bool alwaysShowResults = false)
        {
            var userAnswer = quiz.Answers.FirstOrDefault(a => a.UserId == userId);
            var hasAnswered = userAnswer != null;

            List<DailyQuizResultDto>? results = null;
            if (hasAnswered || alwaysShowResults)
            {
                var totalAnswers = quiz.Answers.Count;
                results = quiz.Options
                    .OrderBy(o => o.DisplayOrder)
                    .Select(o =>
                    {
                        var count = quiz.Answers.Count(a => a.SelectedOptionId == o.Id);
                        return new DailyQuizResultDto
                        {
                            OptionId = o.Id,
                            AnswerCount = count,
                            Percentage = totalAnswers > 0 ? Math.Round((double)count / totalAnswers * 100, 1) : 0,
                            IsCorrect = o.IsCorrect,
                        };
                    }).ToList();
            }

            return new DailyQuizDto
            {
                QuizId = quiz.Id,
                Date = quiz.QuizDate,
                QuestionType = quiz.QuestionType,
                QuestionText = quiz.QuestionText,
                Options = quiz.Options
                    .OrderBy(o => o.DisplayOrder)
                    .Select(o => new DailyQuizOptionDto
                    {
                        OptionId = o.Id,
                        Text = o.Text,
                        CoverUrl = o.CoverUrl,
                    }).ToList(),
                UserSelectedOptionId = userAnswer?.SelectedOptionId,
                UserWasCorrect = userAnswer?.IsCorrect,
                Results = results,
            };
        }
    }
}
