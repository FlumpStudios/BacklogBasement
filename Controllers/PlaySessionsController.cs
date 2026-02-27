using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.Services;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;

namespace BacklogBasement.Controllers
{
    [Route("api/collection/{gameId}/play-sessions")]
    [ApiController]
    [Authorize]
    public class PlaySessionsController : ControllerBase
    {
        private readonly IPlaySessionService _playSessionService;
        private readonly IUserService _userService;
        private readonly ICollectionService _collectionService;
        private readonly IXpService _xpService;

        public PlaySessionsController(IPlaySessionService playSessionService, IUserService userService, ICollectionService collectionService, IXpService xpService)
        {
            _playSessionService = playSessionService;
            _userService = userService;
            _collectionService = collectionService;
            _xpService = xpService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlaySessions(Guid gameId)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                // Verify the game is in the user's collection
                var collectionItem = await _collectionService.GetCollectionItemAsync(userId.Value, gameId);
                if (collectionItem == null)
                {
                    return NotFound(new { error = "Game not found in your collection" });
                }

                var playSessions = await _playSessionService.GetPlaySessionsAsync(userId.Value, gameId);
                return Ok(playSessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving play sessions", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPlaySession(Guid gameId, [FromBody] AddPlaySessionRequest request)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                if (request.DurationMinutes <= 0)
                {
                    return BadRequest(new { error = "Duration must be greater than 0" });
                }

                if (request.PlayedAt > DateTime.UtcNow)
                {
                    return BadRequest(new { error = "PlayedAt date cannot be in the future" });
                }

                var result = await _playSessionService.AddPlaySessionAsync(userId.Value, gameId, request);
                if (await _xpService.TryGrantAsync(userId.Value, "first_session", gameId.ToString(), IXpService.XP_FIRST_SESSION))
                    Response.Headers.Append("X-XP-Awarded", IXpService.XP_FIRST_SESSION.ToString());
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while adding play session", details = ex.Message });
            }
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeletePlaySession(Guid gameId, Guid sessionId)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                // Verify the game is in the user's collection
                var collectionItem = await _collectionService.GetCollectionItemAsync(userId.Value, gameId);
                if (collectionItem == null)
                {
                    return NotFound(new { error = "Game not found in your collection" });
                }

                var result = await _playSessionService.DeletePlaySessionAsync(userId.Value, sessionId);
                if (!result)
                {
                    return NotFound(new { error = "Play session not found or you don't have permission to delete it" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting play session", details = ex.Message });
            }
        }
    }
}