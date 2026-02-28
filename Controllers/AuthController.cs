using System;
using System.Net.Http;
using AspNet.Security.OpenId.Steam;
using BacklogBasement.Models;
using BacklogBasement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BacklogBasement.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IXpService _xpService;
        private readonly ILogger<AuthController> _logger;
        private readonly string _frontendUrl;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IUserService userService, IXpService xpService, ILogger<AuthController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _userService = userService;
            _xpService = xpService;
            _logger = logger;
            _configuration = configuration;
            _frontendUrl = configuration["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
            _httpClientFactory = httpClientFactory;
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

        [HttpGet("login/steam")]
        public IActionResult LoginSteam()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("SteamCallback"),
                Items = { { "Intent", "login" } }
            };
            return Challenge(properties, SteamAuthenticationDefaults.AuthenticationScheme);
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
                        new Claim("GoogleId", user.GoogleSubjectId!)
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

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (await _xpService.TryGrantAsync(user.Id, "daily_login", today, IXpService.XP_DAILY_LOGIN))
                Response.Headers.Append("X-XP-Awarded", IXpService.XP_DAILY_LOGIN.ToString());

            return Ok(new
            {
                id = user.Id.ToString(),
                email = user.Email ?? string.Empty,
                displayName = user.DisplayName,
                googleId = user.GoogleSubjectId,
                steamId = user.SteamId,
                hasSteamLinked = !string.IsNullOrEmpty(user.SteamId),
                twitchId = user.TwitchId,
                hasTwitchLinked = !string.IsNullOrEmpty(user.TwitchId),
                username = user.Username,
                xpInfo = _xpService.ComputeLevel(user.XpTotal)
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
                Items = { { "Intent", "link" }, { "UserId", userId } }
            };
            return Challenge(properties, SteamAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("callback/steam")]
        public async Task<IActionResult> SteamCallback()
        {
            try
            {
                _logger.LogInformation("Steam callback initiated");

                var authenticateResult = await HttpContext.AuthenticateAsync(SteamAuthenticationDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("Steam authentication failed: {Failure}", authenticateResult.Failure?.Message ?? "Unknown");
                    return Redirect($"{_frontendUrl}/collection?steam=error&message=authentication_failed");
                }

                _logger.LogInformation("Steam authentication succeeded");

                // Steam's OpenID returns the Steam ID as a URL like:
                // https://steamcommunity.com/openid/id/76561198012345678
                var steamIdentifier = authenticateResult.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Steam identifier from claims: {SteamIdentifier}", steamIdentifier ?? "null");

                if (string.IsNullOrEmpty(steamIdentifier))
                {
                    _logger.LogWarning("No Steam ID found in claims");
                    return Redirect($"{_frontendUrl}/collection?steam=error&message=no_steam_id");
                }

                var steamId = steamIdentifier.Split('/').Last();
                _logger.LogInformation("Extracted Steam ID: {SteamId}", steamId);

                // Get persona name from claims if available
                var personaName = authenticateResult.Principal?.FindFirst(ClaimTypes.Name)?.Value ?? "Steam User";

                var intent = authenticateResult.Properties?.Items.TryGetValue("Intent", out var intentValue) == true
                    ? intentValue
                    : null;

                if (intent == "login")
                {
                    // Steam standalone login
                    var user = await _userService.GetOrCreateSteamUserAsync(steamId, personaName);
                    _logger.LogInformation("Steam login for user {UserId}", user.Id);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.DisplayName)
                    };
                    if (!string.IsNullOrEmpty(user.GoogleSubjectId))
                        claims.Add(new Claim("GoogleId", user.GoogleSubjectId));

                    var identity = new ClaimsIdentity(claims, "Steam");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("Cookies", principal);
                    _logger.LogInformation("Signed in user {UserId} via Steam login", user.Id);

                    return Redirect($"{_frontendUrl}/?auth=success");
                }
                else
                {
                    // Steam account linking flow
                    var userId = authenticateResult.Properties?.Items.TryGetValue("UserId", out var userIdValue) == true
                        ? userIdValue
                        : null;

                    _logger.LogInformation("User ID from auth properties: {UserId}", userId ?? "null");

                    if (userId == null)
                    {
                        _logger.LogWarning("User not logged in during Steam link callback");
                        return Redirect($"{_frontendUrl}/collection?steam=error&message=not_logged_in");
                    }

                    var user = await _userService.LinkSteamAsync(new Guid(userId), steamId);

                    if (user == null)
                    {
                        _logger.LogWarning("User {UserId} not found during Steam linking", userId);
                        return Redirect($"{_frontendUrl}/collection?steam=error&message=user_not_found");
                    }

                    _logger.LogInformation("Successfully linked Steam ID {SteamId} to user {UserId}", steamId, userId);

                    // Re-sign in to refresh the cookie after Steam auth replaced it
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.DisplayName)
                    };
                    if (!string.IsNullOrEmpty(user.GoogleSubjectId))
                        claims.Add(new Claim("GoogleId", user.GoogleSubjectId));

                    var identity = new ClaimsIdentity(claims, "Google");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("Cookies", principal);
                    _logger.LogInformation("Re-signed in user {UserId} after Steam linking", userId);

                    return Redirect($"{_frontendUrl}/collection?steam=linked");
                }
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

        [HttpGet("login/twitch")]
        public IActionResult LoginTwitch()
        {
            var state = Guid.NewGuid().ToString("N");
            SetTwitchStateCookie($"{state}:login:");
            return Redirect(BuildTwitchAuthUrl(state));
        }

        [HttpGet("link-twitch")]
        [Authorize]
        public IActionResult LinkTwitch()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var state = Guid.NewGuid().ToString("N");
            SetTwitchStateCookie($"{state}:link:{userId}");
            return Redirect(BuildTwitchAuthUrl(state));
        }

        [HttpGet("callback/twitch")]
        public async Task<IActionResult> TwitchCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
                return Redirect($"{_frontendUrl}/?auth=error&message=twitch_denied");

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                return Redirect($"{_frontendUrl}/?auth=error&message=missing_params");

            var stateCookie = Request.Cookies["twitch_state"];
            if (string.IsNullOrEmpty(stateCookie))
                return Redirect($"{_frontendUrl}/?auth=error&message=invalid_state");

            var parts = stateCookie.Split(':', 3);
            if (parts.Length < 3 || parts[0] != state)
                return Redirect($"{_frontendUrl}/?auth=error&message=invalid_state");

            var intent = parts[1];
            var storedUserId = parts[2];

            Response.Cookies.Delete("twitch_state");

            var clientId = _configuration["Igdb:ClientId"]!;
            var clientSecret = _configuration["Igdb:ClientSecret"]!;
            var redirectUri = Url.Action("TwitchCallback", "Auth", null, Request.Scheme)!;

            var tokenResponse = await ExchangeTwitchCodeAsync(code, clientId, clientSecret, redirectUri);
            if (tokenResponse == null)
                return Redirect($"{_frontendUrl}/?auth=error&message=token_exchange_failed");

            var twitchUser = await GetTwitchUserAsync(tokenResponse.AccessToken, clientId);
            if (twitchUser == null)
                return Redirect($"{_frontendUrl}/?auth=error&message=user_fetch_failed");

            if (intent == "login")
            {
                var user = await _userService.GetOrCreateTwitchUserAsync(twitchUser.Id, twitchUser.DisplayName, twitchUser.Email);
                var claims = BuildClaims(user);
                await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(new ClaimsIdentity(claims, "Twitch")));
                return Redirect($"{_frontendUrl}/?auth=success");
            }
            else
            {
                if (!Guid.TryParse(storedUserId, out var userId))
                    return Redirect($"{_frontendUrl}/collection?twitch=error&message=invalid_user");

                try
                {
                    var user = await _userService.LinkTwitchAsync(userId, twitchUser.Id);
                    if (user == null)
                        return Redirect($"{_frontendUrl}/collection?twitch=error&message=user_not_found");

                    var claims = BuildClaims(user);
                    await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(new ClaimsIdentity(claims, "Twitch")));
                    return Redirect($"{_frontendUrl}/collection?twitch=linked");
                }
                catch (InvalidOperationException)
                {
                    return Redirect($"{_frontendUrl}/collection?twitch=error&message=already_linked");
                }
            }
        }

        [HttpPost("unlink-twitch")]
        [Authorize]
        public async Task<IActionResult> UnlinkTwitch()
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            await _userService.UnlinkTwitchAsync(userId.Value);
            return Ok();
        }

        private string BuildTwitchAuthUrl(string state)
        {
            var clientId = _configuration["Igdb:ClientId"]!;
            var redirectUri = Url.Action("TwitchCallback", "Auth", null, Request.Scheme)!;
            var scope = "user:read:email";
            return $"https://id.twitch.tv/oauth2/authorize" +
                   $"?client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&response_type=code" +
                   $"&scope={Uri.EscapeDataString(scope)}" +
                   $"&state={state}";
        }

        private void SetTwitchStateCookie(string value)
        {
            Response.Cookies.Append("twitch_state", value, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(10)
            });
        }

        private static List<Claim> BuildClaims(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.DisplayName)
            };
            if (!string.IsNullOrEmpty(user.GoogleSubjectId))
                claims.Add(new("GoogleId", user.GoogleSubjectId));
            return claims;
        }

        private async Task<TwitchTokenResponse?> ExchangeTwitchCodeAsync(string code, string clientId, string clientSecret, string redirectUri)
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["code"] = code,
                    ["grant_type"] = "authorization_code",
                    ["redirect_uri"] = redirectUri
                }));
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<TwitchTokenResponse>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<TwitchUserInfo?> GetTwitchUserAsync(string accessToken, string clientId)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add("Client-Id", clientId);
            var response = await client.GetAsync("https://api.twitch.tv/helix/users");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0) return null;
            var first = data[0];
            return new TwitchUserInfo(
                first.GetProperty("id").GetString() ?? "",
                first.GetProperty("display_name").GetString() ?? "",
                first.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null
            );
        }

        private sealed class TwitchTokenResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = "";
        }

        private record TwitchUserInfo(string Id, string DisplayName, string? Email);
    }
}