using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [ApiController]
    [Route("api/games/{gameId}/passwords")]
    [Authorize]
    public class GamePasswordController : ControllerBase
    {
        private readonly IGamePasswordService _passwordService;
        private readonly IUserService _userService;

        public GamePasswordController(IGamePasswordService passwordService, IUserService userService)
        {
            _passwordService = passwordService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPasswords(Guid gameId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var passwords = await _passwordService.GetPasswordsAsync(userId.Value, gameId);
            return Ok(passwords);
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicPasswords(Guid gameId)
        {
            var passwords = await _passwordService.GetPublicPasswordsAsync(gameId);
            return Ok(passwords);
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(Guid gameId, [FromBody] CreateGamePasswordRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var password = await _passwordService.AddPasswordAsync(userId.Value, gameId, request);
            return Ok(password);
        }

        [HttpDelete("{passwordId}")]
        public async Task<IActionResult> DeletePassword(Guid gameId, Guid passwordId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            await _passwordService.DeletePasswordAsync(userId.Value, passwordId);
            return NoContent();
        }
    }
}
