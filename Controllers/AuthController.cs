using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.Services;
using System.Threading.Tasks;
using System.Security.Claims;
using BacklogBasement.Models;

namespace BacklogBasement.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly string _frontendUrl;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:5173";
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("login/google")]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("callback/google")]
        public async Task<IActionResult> GoogleCallback()
        {
            // The authentication middleware will handle the callback and populate User.Identity
            if (User.Identity?.IsAuthenticated == true)
            {
                // Get user info from claims
                var googleSubjectId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var displayName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (!string.IsNullOrEmpty(googleSubjectId) && !string.IsNullOrEmpty(email))
                {
                    // Create or get user
                    var user = await _userService.GetOrCreateUserAsync(googleSubjectId, email, displayName ?? email);
                    
                    // Sign in the user with additional claims including our database ID
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.DisplayName),
                        new Claim("GoogleId", user.GoogleSubjectId)
                    };

                    var identity = new ClaimsIdentity(claims, "Google");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("Cookies", principal);
                }

                // Redirect to frontend with success parameter
                return Redirect($"{_frontendUrl}/?auth=success");
            }

            return Redirect($"{_frontendUrl}/?auth=error");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var displayName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return Unauthorized();

            // Get the user and return full user info
            var user = await _userService.GetCurrentUserAsync();
            
            if (user == null)
                return Unauthorized();

            return Ok(new
            {
                id = user.Id.ToString(),
                email = user.Email,
                name = user.DisplayName,
                googleId = user.GoogleSubjectId
            });
        }
    }
}