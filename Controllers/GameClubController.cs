using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [Route("api/clubs")]
    [ApiController]
    [Authorize]
    public class GameClubController : ControllerBase
    {
        private readonly IGameClubService _clubService;
        private readonly IUserService _userService;

        public GameClubController(IGameClubService clubService, IUserService userService)
        {
            _clubService = clubService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicClubs()
        {
            try
            {
                var clubs = await _clubService.GetPublicClubsAsync();
                return Ok(clubs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyClubs()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var clubs = await _clubService.GetMyClubsAsync(userId.Value);
                return Ok(clubs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClub([FromBody] CreateGameClubRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var club = await _clubService.CreateClubAsync(userId.Value, request);
                return Ok(club);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{clubId}")]
        public async Task<IActionResult> GetClubDetail(Guid clubId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var club = await _clubService.GetClubDetailAsync(userId.Value, clubId);
                return Ok(club);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/join")]
        public async Task<IActionResult> JoinPublicClub(Guid clubId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _clubService.JoinPublicClubAsync(userId.Value, clubId);
                return Ok(new { message = "Joined club successfully." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/invite")]
        public async Task<IActionResult> InviteMember(Guid clubId, [FromBody] InviteMemberRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var invite = await _clubService.InviteMemberAsync(userId.Value, clubId, request.InviteeUserId);
                return Ok(invite);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("invites")]
        public async Task<IActionResult> GetMyPendingInvites()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var invites = await _clubService.GetMyPendingInvitesAsync(userId.Value);
                return Ok(invites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/invites/{inviteId}/respond")]
        public async Task<IActionResult> RespondToInvite(Guid clubId, Guid inviteId, [FromBody] RespondToInviteRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _clubService.RespondToInviteAsync(userId.Value, inviteId, request.Accept);
                return Ok(new { message = request.Accept ? "Joined club successfully." : "Invite declined." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{clubId}/members/{targetUserId}")]
        public async Task<IActionResult> RemoveMember(Guid clubId, Guid targetUserId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _clubService.RemoveMemberAsync(userId.Value, clubId, targetUserId);
                return NoContent();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{clubId}/members/{targetUserId}/role")]
        public async Task<IActionResult> UpdateMemberRole(Guid clubId, Guid targetUserId, [FromBody] UpdateMemberRoleRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _clubService.UpdateMemberRoleAsync(userId.Value, clubId, targetUserId, request.Role);
                return Ok(new { message = "Role updated." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/transfer")]
        public async Task<IActionResult> TransferOwnership(Guid clubId, [FromBody] TransferOwnershipRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _clubService.TransferOwnershipAsync(userId.Value, clubId, request.NewOwnerId);
                return Ok(new { message = "Ownership transferred." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{clubId}")]
        public async Task<IActionResult> DeleteClub(Guid clubId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _clubService.DeleteClubAsync(userId.Value, clubId);
                return NoContent();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/rounds")]
        public async Task<IActionResult> StartNewRound(Guid clubId, [FromBody] StartRoundRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var round = await _clubService.StartNewRoundAsync(userId.Value, clubId, request);
                return Ok(round);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/rounds/{roundId}/advance")]
        public async Task<IActionResult> AdvanceRoundStatus(Guid clubId, Guid roundId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var round = await _clubService.AdvanceRoundStatusAsync(userId.Value, roundId);
                return Ok(round);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/rounds/{roundId}/nominate")]
        public async Task<IActionResult> NominateGame(Guid clubId, Guid roundId, [FromBody] NominateGameRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var nomination = await _clubService.NominateGameAsync(userId.Value, roundId, request.GameId);
                return Ok(nomination);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/rounds/{roundId}/vote")]
        public async Task<IActionResult> Vote(Guid clubId, Guid roundId, [FromBody] VoteRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var vote = await _clubService.VoteAsync(userId.Value, roundId, request.NominationId);
                return Ok(vote);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{clubId}/rounds/{roundId}/review")]
        public async Task<IActionResult> SubmitReview(Guid clubId, Guid roundId, [FromBody] SubmitReviewRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var review = await _clubService.SubmitReviewAsync(userId.Value, roundId, request);
                return Ok(review);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{clubId}/rounds/{roundId}/reviews")]
        public async Task<IActionResult> GetRoundReviews(Guid clubId, Guid roundId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var reviews = await _clubService.GetRoundReviewsAsync(userId.Value, roundId);
                return Ok(reviews);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("score/{gameId}")]
        public async Task<IActionResult> GetClubScoreForGame(Guid gameId)
        {
            try
            {
                var score = await _clubService.GetClubScoreForGameAsync(gameId);
                if (score == null) return NoContent();
                return Ok(score);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    // Additional request types (not in DTOs file)
    public class NominateGameRequest
    {
        public Guid GameId { get; set; }
    }

    public class VoteRequest
    {
        public Guid NominationId { get; set; }
    }
}
