using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BacklogBasement.Services
{
    public class SteamService : ISteamService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SteamService> _logger;
        private readonly string _apiKey;
        private const string SteamApiBaseUrl = "https://api.steampowered.com";

        public SteamService(HttpClient httpClient, IConfiguration configuration, ILogger<SteamService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Steam:ApiKey"] ?? throw new ArgumentNullException("Steam:ApiKey configuration is required");
        }

        public async Task<IEnumerable<SteamGame>> GetOwnedGamesAsync(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
            {
                _logger.LogWarning("GetOwnedGamesAsync called with empty steamId");
                return Enumerable.Empty<SteamGame>();
            }

            try
            {
                var url = $"{SteamApiBaseUrl}/IPlayerService/GetOwnedGames/v0001/" +
                          $"?key={_apiKey}" +
                          $"&steamid={steamId}" +
                          "&format=json" +
                          "&include_appinfo=true" +
                          "&include_played_free_games=true";

                _logger.LogInformation("Fetching owned games for Steam ID: {SteamId}", steamId);

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Steam API returned {StatusCode}: {Content}", response.StatusCode, responseContent);
                    return Enumerable.Empty<SteamGame>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<SteamOwnedGamesResponse>(responseContent, options);

                if (result?.Response?.Games == null)
                {
                    _logger.LogWarning("Steam API returned empty or null games list for Steam ID: {SteamId}", steamId);
                    return Enumerable.Empty<SteamGame>();
                }

                var games = result.Response.Games.Select(g => new SteamGame
                {
                    AppId = g.Appid,
                    Name = g.Name ?? $"Unknown Game ({g.Appid})",
                    PlaytimeForever = g.Playtime_Forever,
                    Playtime2Weeks = g.Playtime_2weeks,
                    ImgIconUrl = !string.IsNullOrEmpty(g.Img_Icon_Url)
                        ? $"https://media.steampowered.com/steamcommunity/public/images/apps/{g.Appid}/{g.Img_Icon_Url}.jpg"
                        : null,
                    ImgLogoUrl = !string.IsNullOrEmpty(g.Img_Logo_Url)
                        ? $"https://media.steampowered.com/steamcommunity/public/images/apps/{g.Appid}/{g.Img_Logo_Url}.jpg"
                        : null,
                    // Use Steam CDN header image - more reliable than logo hash
                    HeaderUrl = $"https://cdn.akamai.steamstatic.com/steam/apps/{g.Appid}/header.jpg"
                }).ToList();

                _logger.LogInformation("Successfully fetched {Count} games for Steam ID: {SteamId}", games.Count, steamId);

                return games;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while fetching owned games for Steam ID: {SteamId}", steamId);
                return Enumerable.Empty<SteamGame>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed for Steam owned games response");
                return Enumerable.Empty<SteamGame>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching owned games for Steam ID: {SteamId}", steamId);
                return Enumerable.Empty<SteamGame>();
            }
        }

        public async Task<int?> GetGamePlaytimeAsync(string steamId, long steamAppId)
        {
            var games = await GetOwnedGamesAsync(steamId);
            var game = games.FirstOrDefault(g => g.AppId == steamAppId);
            return game?.PlaytimeForever;
        }

        public async Task<int?> GetMetacriticScoreAsync(long steamAppId)
        {
            var (score, _) = await GetSteamAppDetailsAsync(steamAppId);
            return score;
        }

        public async Task<(int? MetacriticScore, string? Description)> GetSteamAppDetailsAsync(long steamAppId)
        {
            try
            {
                var url = $"https://store.steampowered.com/api/appdetails?appids={steamAppId}";
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (null, null);

                using var doc = JsonDocument.Parse(content);
                var appData = doc.RootElement.GetProperty(steamAppId.ToString());

                if (!appData.GetProperty("success").GetBoolean())
                    return (null, null);

                var data = appData.GetProperty("data");

                int? score = null;
                if (data.TryGetProperty("metacritic", out var metacritic))
                    score = metacritic.GetProperty("score").GetInt32();

                string? description = null;
                if (data.TryGetProperty("short_description", out var descProp))
                {
                    var raw = descProp.GetString();
                    if (!string.IsNullOrWhiteSpace(raw))
                        description = raw;
                }

                return (score, description);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Steam app details for app {AppId}", steamAppId);
                return (null, null);
            }
        }
    }
}
