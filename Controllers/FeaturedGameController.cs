using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [ApiController]
    public class FeaturedGameController : ControllerBase
    {
        private readonly IFeaturedGameService _featuredService;
        private readonly IUserService _userService;

        public FeaturedGameController(IFeaturedGameService featuredService, IUserService userService)
        {
            _featuredService = featuredService;
            _userService = userService;
        }

        [HttpGet("api/featured")]
        public async Task<IActionResult> GetFeatured()
        {
            var games = await _featuredService.GetFeaturedAsync();
            return Ok(games);
        }

        [HttpGet("api/admin/games/search")]
        [Authorize]
        public async Task<IActionResult> SearchGames([FromQuery] string q)
        {
            if (!await IsAdminAsync()) return Forbid();
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());
            var results = await _featuredService.SearchGamesInDbAsync(q);
            return Ok(results);
        }

        [HttpPost("api/admin/featured/{gameId}")]
        [Authorize]
        public async Task<IActionResult> AddFeatured(Guid gameId)
        {
            if (!await IsAdminAsync()) return Forbid();
            await _featuredService.AddFeaturedAsync(gameId);
            return Ok();
        }

        [HttpDelete("api/admin/featured/{gameId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFeatured(Guid gameId)
        {
            if (!await IsAdminAsync()) return Forbid();
            await _featuredService.RemoveFeaturedAsync(gameId);
            return NoContent();
        }

        private async Task<bool> IsAdminAsync()
        {
            var user = await _userService.GetCurrentUserAsync();
            return user?.IsAdmin == true;
        }
    }
}
