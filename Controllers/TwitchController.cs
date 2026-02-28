using System.Threading.Tasks;
using BacklogBasement.Services;
using Microsoft.AspNetCore.Mvc;

namespace BacklogBasement.Controllers
{
    [Route("api/twitch")]
    [ApiController]
    public class TwitchController : ControllerBase
    {
        private readonly ITwitchService _twitchService;

        public TwitchController(ITwitchService twitchService)
        {
            _twitchService = twitchService;
        }

        [HttpGet("streams/{igdbId}")]
        public async Task<IActionResult> GetStreams(long igdbId)
        {
            var streams = await _twitchService.GetLiveStreamsForGameAsync(igdbId);
            return Ok(streams);
        }
    }
}
