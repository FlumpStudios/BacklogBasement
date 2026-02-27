using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Services;
using BacklogBasement.Exceptions;

namespace BacklogBasement.Controllers
{
    [Route("api/poll")]
    [ApiController]
    [Authorize]
    public class DailyPollController : ControllerBase
    {
        private readonly IDailyPollService _pollService;
        private readonly IUserService _userService;
        private readonly IXpService _xpService;

        public DailyPollController(IDailyPollService pollService, IUserService userService, IXpService xpService)
        {
            _pollService = pollService;
            _userService = userService;
            _xpService = xpService;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var poll = await _pollService.GetOrCreateTodaysPollAsync(userId.Value);
                return Ok(poll);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred retrieving today's poll", details = ex.Message });
            }
        }

        [HttpGet("previous")]
        public async Task<IActionResult> GetPrevious()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var poll = await _pollService.GetPreviousPollAsync(userId.Value);
                if (poll == null) return NoContent();
                return Ok(poll);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred retrieving the previous poll", details = ex.Message });
            }
        }

        [HttpPost("{pollId}/vote")]
        public async Task<IActionResult> Vote(Guid pollId, [FromBody] VotePollRequest request)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var result = await _pollService.VoteAsync(userId.Value, pollId, request.GameId);

                if (await _xpService.TryGrantAsync(userId.Value, "daily_poll", pollId.ToString(), IXpService.XP_DAILY_POLL))
                    Response.Headers.Append("X-XP-Awarded", IXpService.XP_DAILY_POLL.ToString());

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
                return StatusCode(500, new { error = "An error occurred while voting", details = ex.Message });
            }
        }
    }
}
