using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [Route("api/activity")]
    [ApiController]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int limit = 50)
        {
            limit = Math.Clamp(limit, 1, 100);
            var events = await _activityService.GetFeedAsync(limit);
            return Ok(events);
        }
    }
}
