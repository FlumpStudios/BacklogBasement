using System;
using System.Threading.Tasks;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Models;
using BacklogBasement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BacklogBasement.Controllers
{
    [Route("api/twitch")]
    [ApiController]
    public class TwitchController : ControllerBase
    {
        private readonly ITwitchService _twitchService;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TwitchController> _logger;

        public TwitchController(
            ITwitchService twitchService,
            IUserService userService,
            IGameService gameService,
            ApplicationDbContext context,
            ILogger<TwitchController> logger)
        {
            _twitchService = twitchService;
            _userService = userService;
            _gameService = gameService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("streams/{igdbId}")]
        public async Task<IActionResult> GetStreams(long igdbId)
        {
            var streams = await _twitchService.GetLiveStreamsForGameAsync(igdbId);
            return Ok(streams);
        }

        /// <summary>Public — shows if a Twitch user is currently live.</summary>
        [HttpGet("live/{twitchUserId}")]
        public async Task<IActionResult> GetLiveStatus(string twitchUserId)
        {
            var liveStatus = await _twitchService.GetLiveStreamAsync(twitchUserId);
            return Ok(liveStatus);
        }

        /// <summary>Import all games the current user has ever streamed on Twitch.</summary>
        [HttpPost("import")]
        [Authorize]
        public async Task<IActionResult> ImportStreams()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userService.GetCurrentUserAsync();
            if (string.IsNullOrEmpty(user?.TwitchId))
                return BadRequest(new { message = "No Twitch account linked" });

            var streamedGames = await _twitchService.GetStreamedGamesAsync(user.TwitchId);
            var result = new TwitchImportResultDto { Total = streamedGames.Count };

            foreach (var sg in streamedGames)
            {
                try
                {
                    var game = await _gameService.GetOrFetchGameFromIgdbAsync(sg.IgdbId);

                    var existing = await _context.UserGames
                        .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == game.Id);

                    if (existing != null)
                    {
                        result.Skipped++;
                        continue;
                    }

                    var userGame = new UserGame
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId.Value,
                        GameId = game.Id,
                        DateAdded = DateTime.UtcNow,
                        Status = null
                    };
                    _context.UserGames.Add(userGame);
                    await _context.SaveChangesAsync();

                    if (sg.TotalMinutes > 0)
                    {
                        _context.PlaySessions.Add(new PlaySession
                        {
                            Id = Guid.NewGuid(),
                            UserGameId = userGame.Id,
                            DurationMinutes = sg.TotalMinutes,
                            PlayedAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }

                    result.Imported++;
                    result.ImportedGames.Add(new TwitchImportedGameDto
                    {
                        Name = game.Name,
                        IgdbId = sg.IgdbId,
                        StreamedMinutes = sg.TotalMinutes
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import Twitch game {IgdbId}", sg.IgdbId);
                    result.Skipped++;
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Check if the current user is live on Twitch.
        /// If they are, auto-update (or add) the game to their collection as "playing".
        /// </summary>
        [HttpPost("sync-now")]
        [Authorize]
        public async Task<IActionResult> SyncNow()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userService.GetCurrentUserAsync();
            if (string.IsNullOrEmpty(user?.TwitchId))
                return Ok(new TwitchLiveDto { IsLive = false });

            var liveStatus = await _twitchService.GetLiveStreamAsync(user.TwitchId);

            if (liveStatus.IsLive && liveStatus.IgdbGameId.HasValue)
            {
                try
                {
                    var game = await _gameService.GetOrFetchGameFromIgdbAsync(liveStatus.IgdbGameId.Value);
                    var userGame = await _context.UserGames
                        .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == game.Id);

                    if (userGame == null)
                    {
                        _context.UserGames.Add(new UserGame
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId.Value,
                            GameId = game.Id,
                            DateAdded = DateTime.UtcNow,
                            Status = "playing"
                        });
                        await _context.SaveChangesAsync();
                        liveStatus.UpdatedPlayingStatus = true;
                    }
                    else if (userGame.Status != "playing")
                    {
                        userGame.Status = "playing";
                        await _context.SaveChangesAsync();
                        liveStatus.UpdatedPlayingStatus = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync live game for user {UserId}", userId);
                }
            }

            return Ok(liveStatus);
        }
    }
}
