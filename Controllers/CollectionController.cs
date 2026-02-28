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
    [Route("api/collection")]
    [ApiController]
    [Authorize]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionService _collectionService;
        private readonly IUserService _userService;
        private readonly IXpService _xpService;

        public CollectionController(ICollectionService collectionService, IUserService userService, IXpService xpService)
        {
            _collectionService = collectionService;
            _userService = userService;
            _xpService = xpService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCollection()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                var collection = await _collectionService.GetUserCollectionAsync(userId.Value);
                
                // Calculate total playtime for each item
                var collectionWithPlayTime = new List<object>();
                foreach (var item in collection)
                {
                    item.TotalPlayTimeMinutes = await GetTotalPlayTimeAsync(userId.Value, item.GameId);
                    collectionWithPlayTime.Add(item);
                }

                return Ok(collection);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving collection", details = ex.Message });
            }
        }

        [HttpPost("{gameId}")]
        public async Task<IActionResult> AddToCollection(Guid gameId, [FromBody] AddToCollectionRequest request)
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
                    request = new AddToCollectionRequest { GameId = gameId };
                }
                else
                {
                    request.GameId = gameId;
                }

                var result = await _collectionService.AddGameToCollectionAsync(userId.Value, request);
                var xpAwarded = 0;
                if (await _xpService.TryGrantAsync(userId.Value, "first_game_added", "initial", IXpService.XP_FIRST_GAME))
                    xpAwarded += IXpService.XP_FIRST_GAME;
                if (result.Status == "backlog" && await _xpService.TryGrantAsync(userId.Value, "add_to_backlog", "initial", IXpService.XP_ADD_TO_BACKLOG))
                    xpAwarded += IXpService.XP_ADD_TO_BACKLOG;
                if (xpAwarded > 0)
                    Response.Headers.Append("X-XP-Awarded", xpAwarded.ToString());
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
                return StatusCode(500, new { error = "An error occurred while adding to collection", details = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetCollectionStats()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized(new { error = "User not found" });
            var stats = await _collectionService.GetCollectionStatsAsync(userId.Value);
            return Ok(stats);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedCollection(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? source = null,
            [FromQuery] string? playStatus = null,
            [FromQuery] string sortBy = "added",
            [FromQuery] string sortDir = "desc")
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null) return Unauthorized(new { error = "User not found" });
                take = Math.Clamp(take, 1, 200);
                var result = await _collectionService.GetPagedCollectionAsync(userId.Value, skip, take, search, status, source, playStatus, sortBy, sortDir);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving collection", details = ex.Message });
            }
        }

        [HttpPost("bulk-add")]
        public async Task<IActionResult> BulkAddToCollection([FromBody] BulkAddRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized(new { error = "User not found" });
            if (request?.GameIds == null || request.GameIds.Count == 0)
                return BadRequest(new { error = "GameIds are required" });

            try
            {
                var (added, alreadyOwned) = await _collectionService.BulkAddGamesAsync(userId.Value, request.GameIds);

                var xpAwarded = 0;
                if (await _xpService.TryGrantAsync(userId.Value, "retroarch_import", "initial", IXpService.XP_RETROARCH_IMPORT))
                    xpAwarded += IXpService.XP_RETROARCH_IMPORT;
                if (xpAwarded > 0)
                    Response.Headers.Append("X-XP-Awarded", xpAwarded.ToString());

                return Ok(new { added, alreadyOwned });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred during bulk add", details = ex.Message });
            }
        }

        [HttpDelete("{gameId}")]
        public async Task<IActionResult> RemoveFromCollection(Guid gameId)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                var success = await _collectionService.RemoveGameFromCollectionAsync(userId.Value, gameId);
                if (!success)
                {
                    return NotFound(new { error = "Game not found in collection" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while removing from collection", details = ex.Message });
            }
        }

        [HttpPatch("{gameId}/status")]
        public async Task<ActionResult<CollectionItemDto>> UpdateStatus(Guid gameId, [FromBody] UpdateGameStatusRequest request)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                var result = await _collectionService.UpdateGameStatusAsync(userId.Value, gameId, request.Status);
                var xpAwarded = 0;
                if (request.Status == "completed" && await _xpService.TryGrantAsync(userId.Value, "complete_game", gameId.ToString(), IXpService.XP_COMPLETE_GAME))
                    xpAwarded += IXpService.XP_COMPLETE_GAME;
                if (request.Status == "backlog" && await _xpService.TryGrantAsync(userId.Value, "add_to_backlog", "initial", IXpService.XP_ADD_TO_BACKLOG))
                    xpAwarded += IXpService.XP_ADD_TO_BACKLOG;
                if (xpAwarded > 0)
                    Response.Headers.Append("X-XP-Awarded", xpAwarded.ToString());
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
                return StatusCode(500, new { error = "An error occurred while updating status", details = ex.Message });
            }
        }

        private async Task<int> GetTotalPlayTimeAsync(Guid userId, Guid gameId)
        {
            // This is a simplified version - in a real app, you might want to cache this
            var sessions = await _collectionService.GetCollectionItemAsync(userId, gameId);
            return sessions?.TotalPlayTimeMinutes ?? 0;
        }
    }
}