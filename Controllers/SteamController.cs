using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [Route("api/steam")]
    [ApiController]
    [Authorize]
    public class SteamController : ControllerBase
    {
        private readonly ISteamImportService _steamImportService;
        private readonly IUserService _userService;
        private readonly IXpService _xpService;

        public SteamController(ISteamImportService steamImportService, IUserService userService, IXpService xpService)
        {
            _steamImportService = steamImportService;
            _userService = userService;
            _xpService = xpService;
        }

        [HttpGet("status")]
        public async Task<ActionResult<SteamStatusDto>> GetStatus()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var status = await _steamImportService.GetSteamStatusAsync(userId.Value);
            return Ok(status);
        }

        [HttpPost("import")]
        public async Task<ActionResult<SteamImportResult>> ImportLibrary([FromBody] SteamImportRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _steamImportService.ImportLibraryAsync(userId.Value, request.IncludePlaytime);
                if (await _xpService.TryGrantAsync(userId.Value, "steam_import", "initial", IXpService.XP_STEAM_IMPORT))
                    Response.Headers.Append("X-XP-Awarded", IXpService.XP_STEAM_IMPORT.ToString());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("sync-all-playtime")]
        public async Task<ActionResult<SteamBulkPlaytimeSyncResult>> SyncAllPlaytime()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _steamImportService.SyncAllPlaytimesAsync(userId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("unlink")]
        public async Task<IActionResult> UnlinkSteam()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _userService.UnlinkSteamAsync(userId.Value);
            if (user == null)
                return NotFound();

            return Ok(new { message = "Steam account unlinked successfully" });
        }

        [HttpPost("{gameId}/sync-playtime")]
        public async Task<ActionResult<SteamPlaytimeSyncResult>> SyncPlaytime(Guid gameId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _steamImportService.SyncGamePlaytimeAsync(userId.Value, gameId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
