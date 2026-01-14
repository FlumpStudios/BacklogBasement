using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspNet.Security.OpenId.Steam;
using BacklogBasement.Services;
using System.Threading.Tasks;
using System.Security.Claims;
using BacklogBasement.Models;
using System.Linq;

namespace BacklogBasement.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly string _frontendUrl;

        public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
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

            if (string.IsNullOrEmpty(userId))
                return Forbid();

            // Get the user and return full user info
            var user = await _userService.GetCurrentUserAsync();
            
            if (user == null)
                return Forbid();

            return Ok(new
            {
                id = user.Id.ToString(),
                email = user.Email ?? string.Empty,
                name = user.DisplayName,
                googleId = user.GoogleSubjectId,
                steamId = user.SteamId,
                hasSteamLinked = !string.IsNullOrEmpty(user.SteamId)
            });
        }

        [HttpGet("link-steam")]
        [Authorize]
        public IActionResult LinkSteam()
        {
            // Store the user's ID in the authentication properties so we can use it in the callback
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("SteamCallback"),
                Items = { { "UserId", userId } }
            };
            return Challenge(properties, SteamAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("callback/steam")]
        public async Task<IActionResult> SteamCallback()
        {
            try
            {
                _logger.LogInformation("Steam callback initiated");

                // Authenticate with Steam to get the Steam identity
                var authenticateResult = await HttpContext.AuthenticateAsync(SteamAuthenticationDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("Steam authentication failed: {Failure}", authenticateResult.Failure?.Message ?? "Unknown");
                    return Redirect($"{_frontendUrl}/collection?steam=error&message=authentication_failed");
                }

                _logger.LogInformation("Steam authentication succeeded");

                // Get the Steam ID from the claims
                // Steam's OpenID returns the Steam ID in the NameIdentifier claim as a URL like:
                // https://steamcommunity.com/openid/id/76561198012345678
                var steamIdentifier = authenticateResult.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation("Steam identifier from claims: {SteamIdentifier}", steamIdentifier ?? "null");

                if (string.IsNullOrEmpty(steamIdentifier))
                {
                    _logger.LogWarning("No Steam ID found in claims");
                    return Redirect($"{_frontendUrl}/collection?steam=error&message=no_steam_id");
                }

                // Extract the Steam ID from the URL (last segment)
                var steamId = steamIdentifier.Split('/').Last();
                _logger.LogInformation("Extracted Steam ID: {SteamId}", steamId);

                // Get the current user's ID from the authentication properties
                var userId = authenticateResult.Properties.Items["UserId"];
                _logger.LogInformation("User ID from auth properties: {UserId}", userId?.ToString() ?? "null");

                if (userId == null)
                {
                    _logger.LogWarning("User not logged in during Steam callback");
                    return Redirect($"{_frontendUrl}/collection?steam=error&message=not_logged_in");
                }

                // Link the Steam account to the user
                var user = await _userService.LinkSteamAsync(new Guid(userId), steamId);

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found during Steam linking", userId);
                    return Redirect($"{_frontendUrl}/collection?steam=error&message=user_not_found");
                }

                _logger.LogInformation("Successfully linked Steam ID {SteamId} to user {UserId}", steamId, userId);

                // Re-sign in the user with their original Google credentials
                // This is necessary because Steam auth replaced the cookie
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
                _logger.LogInformation("Re-signed in user {UserId} after Steam linking", userId);

                return Redirect($"{_frontendUrl}/collection?steam=linked");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Steam account already linked");
                return Redirect($"{_frontendUrl}/collection?steam=error&message=already_linked");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Steam callback");
                return Redirect($"{_frontendUrl}/collection?steam=error&message=unexpected_error");
            }
        }

        [HttpDelete("unlink-steam")]
        [Authorize]
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
    }
}