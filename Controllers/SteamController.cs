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

        public SteamController(ISteamImportService steamImportService, IUserService userService)
        {
            _steamImportService = steamImportService;
            _userService = userService;
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
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
