using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.Services;
using BacklogBasement.Exceptions;

namespace BacklogBasement.Controllers
{
    [Route("api/friends")]
    [ApiController]
    [Authorize]
    public class FriendshipController : ControllerBase
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IUserService _userService;

        public FriendshipController(IFriendshipService friendshipService, IUserService userService)
        {
            _friendshipService = friendshipService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var friends = await _friendshipService.GetFriendsAsync(userId.Value);
                return Ok(friends);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving friends", details = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPlayers([FromQuery] string q)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                    return Ok(Array.Empty<object>());

                var results = await _friendshipService.SearchPlayersAsync(q, userId.Value);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while searching players", details = ex.Message });
            }
        }

        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetFriendshipStatus(Guid userId)
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { error = "User not found" });

                var status = await _friendshipService.GetFriendshipStatusAsync(currentUserId.Value, userId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while checking friendship status", details = ex.Message });
            }
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var requests = await _friendshipService.GetPendingRequestsAsync(userId.Value);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving friend requests", details = ex.Message });
            }
        }

        [HttpPost("request/{userId}")]
        public async Task<IActionResult> SendFriendRequest(Guid userId)
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { error = "User not found" });

                var result = await _friendshipService.SendFriendRequestAsync(currentUserId.Value, userId);
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
                return StatusCode(500, new { error = "An error occurred while sending friend request", details = ex.Message });
            }
        }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptFriendRequest(Guid id)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                await _friendshipService.AcceptFriendRequestAsync(userId.Value, id);
                return Ok(new { message = "Friend request accepted" });
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
                return StatusCode(500, new { error = "An error occurred while accepting friend request", details = ex.Message });
            }
        }

        [HttpPost("{id}/decline")]
        public async Task<IActionResult> DeclineFriendRequest(Guid id)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                await _friendshipService.DeclineFriendRequestAsync(userId.Value, id);
                return Ok(new { message = "Friend request declined" });
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
                return StatusCode(500, new { error = "An error occurred while declining friend request", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFriend(Guid id)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                await _friendshipService.RemoveFriendAsync(userId.Value, id);
                return NoContent();
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
                return StatusCode(500, new { error = "An error occurred while removing friend", details = ex.Message });
            }
        }
    }
}
