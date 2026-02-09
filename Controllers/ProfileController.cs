using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IUserService _userService;

        public ProfileController(IProfileService profileService, IUserService userService)
        {
            _profileService = profileService;
            _userService = userService;
        }

        [HttpGet("{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(string username)
        {
            var profile = await _profileService.GetProfileByUsernameAsync(username);
            if (profile == null)
                return NotFound(new { error = "Profile not found" });

            return Ok(profile);
        }

        [HttpGet("check-username/{username}")]
        [Authorize]
        public async Task<IActionResult> CheckUsername(string username)
        {
            try
            {
                var available = await _userService.IsUsernameAvailableAsync(username);
                return Ok(new UsernameAvailabilityResponse
                {
                    Available = available,
                    Username = username
                });
            }
            catch (BadRequestException)
            {
                return Ok(new UsernameAvailabilityResponse
                {
                    Available = false,
                    Username = username
                });
            }
        }

        [HttpPost("set-username")]
        [Authorize]
        public async Task<IActionResult> SetUsername([FromBody] SetUsernameRequest request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _userService.SetUsernameAsync(userId.Value, request.Username);
            return Ok(new { username = user.Username });
        }
    }
}
