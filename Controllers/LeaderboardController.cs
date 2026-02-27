using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [Route("api/leaderboard")]
    [ApiController]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly IUserService _userService;

        public LeaderboardController(ILeaderboardService leaderboardService, IUserService userService)
        {
            _leaderboardService = leaderboardService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGlobal()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var entries = await _leaderboardService.GetGlobalLeaderboardAsync(userId.Value);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred retrieving the leaderboard", details = ex.Message });
            }
        }

        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var entries = await _leaderboardService.GetFriendLeaderboardAsync(userId.Value);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred retrieving the friends leaderboard", details = ex.Message });
            }
        }
    }
}
