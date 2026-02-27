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
    public class DailyPollService : IDailyPollService
    {
        private static readonly string[] Categories =
        {
            "Best Game",
            "Best Art Style",
            "Most Replayable",
            "Most Overrated",
            "Most Underrated",
            "Best Soundtrack",
            "Most Fun",
            "Most Addictive",
            "Most Emotional",
            "If you had to keep one game",
            "If you had to remove one game from existence",
        };

        private readonly ApplicationDbContext _context;

        public DailyPollService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DailyPollDto> GetOrCreateTodaysPollAsync(Guid userId)
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var poll = await LoadPollAsync(today);

            // If today's poll was created when no games existed, delete it and recreate
            if (poll != null && !poll.Games.Any())
            {
                _context.DailyPolls.Remove(poll);
                await _context.SaveChangesAsync();
                poll = null;
            }

            if (poll == null)
            {
                poll = await CreateTodaysPollAsync(today);
            }

            return MapToDto(poll, userId);
        }

        public async Task<DailyPollDto?> GetPreviousPollAsync(Guid userId)
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var poll = await _context.DailyPolls
                .Include(p => p.Games).ThenInclude(g => g.Game)
                .Include(p => p.Votes)
                .Where(p => p.PollDate != today)
                .OrderByDescending(p => p.PollDate)
                .FirstOrDefaultAsync();

            if (poll == null || !poll.Games.Any()) return null;

            return MapToDto(poll, userId, alwaysShowResults: true);
        }

        public async Task<DailyPollDto> VoteAsync(Guid userId, Guid pollId, Guid gameId)
        {
            var poll = await _context.DailyPolls
                .Include(p => p.Games).ThenInclude(g => g.Game)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll == null)
                throw new NotFoundException("Poll not found.");

            if (!poll.Games.Any(g => g.GameId == gameId))
                throw new BadRequestException("Game is not part of this poll.");

            if (poll.Votes.Any(v => v.UserId == userId))
                throw new BadRequestException("You have already voted in this poll.");

            var vote = new DailyPollVote
            {
                Id = Guid.NewGuid(),
                PollId = pollId,
                UserId = userId,
                VotedGameId = gameId,
                CreatedAt = DateTime.UtcNow
            };

            _context.DailyPollVotes.Add(vote);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Unique index violation — already voted (race condition)
                throw new BadRequestException("You have already voted in this poll.");
            }

            // Reload to get full vote counts
            poll = await LoadPollAsync(poll.PollDate) ?? poll;

            return MapToDto(poll, userId);
        }

        private async Task<DailyPoll?> LoadPollAsync(string pollDate)
        {
            return await _context.DailyPolls
                .Include(p => p.Games).ThenInclude(g => g.Game)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.PollDate == pollDate);
        }

        private async Task<DailyPoll> CreateTodaysPollAsync(string today)
        {
            // Deterministic seed from the date string — same day always gives same poll
            var seed = int.Parse(today.Replace("-", ""));
            var rng = new Random(seed);

            // Pick category
            var category = Categories[seed % Categories.Length];

            // Prefer games that appear in ≥2 users' collections
            var popularGameIds = await _context.UserGames
                .GroupBy(ug => ug.GameId)
                .Where(g => g.Count() >= 2)
                .Select(g => g.Key)
                .ToListAsync();

            List<Guid> candidateIds;
            if (popularGameIds.Count >= 5)
            {
                candidateIds = popularGameIds;
            }
            else
            {
                // Fall back to any games
                candidateIds = await _context.Games
                    .Select(g => g.Id)
                    .ToListAsync();
            }

            if (candidateIds.Count < 5)
            {
                // Not enough games in the database — return an empty/null poll
                // We still need to create a poll; just use what we have (could be < 5)
                // For now, shuffle all and take up to 5
            }

            // Shuffle deterministically and take 5
            var shuffled = candidateIds.OrderBy(_ => rng.Next()).Take(5).ToList();

            var poll = new DailyPoll
            {
                Id = Guid.NewGuid(),
                PollDate = today,
                Category = category,
                CreatedAt = DateTime.UtcNow,
            };

            _context.DailyPolls.Add(poll);

            foreach (var gameId in shuffled)
            {
                _context.DailyPollGames.Add(new DailyPollGame
                {
                    Id = Guid.NewGuid(),
                    PollId = poll.Id,
                    GameId = gameId,
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Race condition — another request already created today's poll
                var existing = await LoadPollAsync(today);
                if (existing != null) return existing;
                throw;
            }

            // Reload with navigation properties
            return (await LoadPollAsync(today))!;
        }

        private static DailyPollDto MapToDto(DailyPoll poll, Guid userId, bool alwaysShowResults = false)
        {
            var userVote = poll.Votes.FirstOrDefault(v => v.UserId == userId);
            var hasVoted = userVote != null;

            List<PollResultDto>? results = null;
            if (hasVoted || alwaysShowResults)
            {
                var totalVotes = poll.Votes.Count;
                results = poll.Games.Select(g =>
                {
                    var voteCount = poll.Votes.Count(v => v.VotedGameId == g.GameId);
                    return new PollResultDto
                    {
                        GameId = g.GameId,
                        VoteCount = voteCount,
                        Percentage = totalVotes > 0 ? Math.Round((double)voteCount / totalVotes * 100, 1) : 0,
                    };
                }).ToList();
            }

            return new DailyPollDto
            {
                PollId = poll.Id,
                Date = poll.PollDate,
                Category = poll.Category,
                Games = poll.Games.Select(g => new DailyPollGameDto
                {
                    GameId = g.GameId,
                    Name = g.Game.Name,
                    CoverUrl = g.Game.CoverUrl,
                }).ToList(),
                UserVotedGameId = userVote?.VotedGameId,
                Results = results,
            };
        }
    }
}
