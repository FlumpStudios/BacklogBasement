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

        public CollectionController(ICollectionService collectionService, IUserService userService)
        {
            _collectionService = collectionService;
            _userService = userService;
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

        private async Task<int> GetTotalPlayTimeAsync(Guid userId, Guid gameId)
        {
            // This is a simplified version - in a real app, you might want to cache this
            var sessions = await _collectionService.GetCollectionItemAsync(userId, gameId);
            return sessions?.TotalPlayTimeMinutes ?? 0;
        }
    }
}