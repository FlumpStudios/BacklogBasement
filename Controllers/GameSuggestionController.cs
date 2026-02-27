using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Services;
using BacklogBasement.Exceptions;

namespace BacklogBasement.Controllers
{
    [Route("api/suggestions")]
    [ApiController]
    [Authorize]
    public class GameSuggestionController : ControllerBase
    {
        private readonly IGameSuggestionService _suggestionService;
        private readonly IUserService _userService;
        private readonly IXpService _xpService;

        public GameSuggestionController(IGameSuggestionService suggestionService, IUserService userService, IXpService xpService)
        {
            _suggestionService = suggestionService;
            _userService = userService;
            _xpService = xpService;
        }

        [HttpPost]
        public async Task<IActionResult> SendSuggestion([FromBody] SendGameSuggestionRequest request)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var result = await _suggestionService.SendSuggestionAsync(userId.Value, request);
                if (await _xpService.TryGrantAsync(userId.Value, "send_suggestion", "initial", IXpService.XP_SEND_SUGGESTION))
                    Response.Headers.Append("X-XP-Awarded", IXpService.XP_SEND_SUGGESTION.ToString());
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
                return StatusCode(500, new { error = "An error occurred while sending suggestion", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReceivedSuggestions()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var suggestions = await _suggestionService.GetReceivedSuggestionsAsync(userId.Value);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving suggestions", details = ex.Message });
            }
        }

        [HttpPost("{id}/dismiss")]
        public async Task<IActionResult> DismissSuggestion(Guid id)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                await _suggestionService.DismissSuggestionAsync(userId.Value, id);
                return Ok(new { message = "Suggestion dismissed" });
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
                return StatusCode(500, new { error = "An error occurred while dismissing suggestion", details = ex.Message });
            }
        }
    }
}
