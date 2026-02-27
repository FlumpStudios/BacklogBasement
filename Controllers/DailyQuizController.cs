using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Services;
using BacklogBasement.Exceptions;

namespace BacklogBasement.Controllers
{
    [Route("api/quiz")]
    [ApiController]
    [Authorize]
    public class DailyQuizController : ControllerBase
    {
        private readonly IDailyQuizService _quizService;
        private readonly IUserService _userService;
        private readonly IXpService _xpService;

        public DailyQuizController(IDailyQuizService quizService, IUserService userService, IXpService xpService)
        {
            _quizService = quizService;
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

                var quiz = await _quizService.GetOrCreateTodaysQuizAsync(userId.Value);
                if (quiz == null) return NoContent();
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred retrieving today's quiz", details = ex.Message });
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

                var quiz = await _quizService.GetPreviousQuizAsync(userId.Value);
                if (quiz == null) return NoContent();
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred retrieving the previous quiz", details = ex.Message });
            }
        }

        [HttpPost("{quizId}/answer")]
        public async Task<IActionResult> Answer(Guid quizId, [FromBody] AnswerQuizRequest request)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var result = await _quizService.AnswerAsync(userId.Value, quizId, request.OptionId);

                var xpReason = result.UserWasCorrect == true ? "quiz_correct" : "quiz_incorrect";
                var xpAmount = result.UserWasCorrect == true ? IXpService.XP_QUIZ_CORRECT : IXpService.XP_QUIZ_INCORRECT;

                if (await _xpService.TryGrantAsync(userId.Value, xpReason, quizId.ToString(), xpAmount))
                    Response.Headers.Append("X-XP-Awarded", xpAmount.ToString());

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
                return StatusCode(500, new { error = "An error occurred while answering the quiz", details = ex.Message });
            }
        }
    }
}
