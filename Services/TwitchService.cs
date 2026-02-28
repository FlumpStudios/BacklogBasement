using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BacklogBasement.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BacklogBasement.Services
{
    public class TwitchService : ITwitchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TwitchService> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _accessToken;
        private DateTime _tokenExpiry;

        public TwitchService(HttpClient httpClient, IConfiguration configuration, ILogger<TwitchService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _clientId = configuration["Igdb:ClientId"] ?? throw new ArgumentNullException("Igdb:ClientId");
            _clientSecret = configuration["Igdb:ClientSecret"] ?? throw new ArgumentNullException("Igdb:ClientSecret");
        }

        public async Task<IEnumerable<TwitchStreamDto>> GetLiveStreamsForGameAsync(long igdbId, int limit = 6)
        {
            try
            {
                await EnsureAccessTokenAsync();

                var url = $"https://api.twitch.tv/helix/streams?game_id={igdbId}&first={limit}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Client-ID", _clientId);
                request.Headers.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                var result = JsonSerializer.Deserialize<TwitchStreamsResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var streams = new List<TwitchStreamDto>();
                foreach (var s in result?.Data ?? [])
                {
                    streams.Add(new TwitchStreamDto
                    {
                        Login = s.UserLogin,
                        UserName = s.UserName,
                        Title = s.Title,
                        ViewerCount = s.ViewerCount,
                        ThumbnailUrl = s.ThumbnailUrl
                            .Replace("{width}", "320")
                            .Replace("{height}", "180"),
                    });
                }
                return streams;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Twitch streams for game {IgdbId}", igdbId);
                return [];
            }
        }

        private async Task EnsureAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow.AddMinutes(5))
                return;

            var body = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
            ]);

            var response = await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", body);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var token = JsonSerializer.Deserialize<TokenResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (token == null) throw new InvalidOperationException("Failed to deserialize Twitch token");

            _accessToken = token.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
        }

        private class TwitchStreamsResponse
        {
            [JsonPropertyName("data")]
            public List<TwitchStreamData> Data { get; set; } = new();
        }

        private class TwitchStreamData
        {
            [JsonPropertyName("user_login")]
            public string UserLogin { get; set; } = string.Empty;
            [JsonPropertyName("user_name")]
            public string UserName { get; set; } = string.Empty;
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;
            [JsonPropertyName("viewer_count")]
            public int ViewerCount { get; set; }
            [JsonPropertyName("thumbnail_url")]
            public string ThumbnailUrl { get; set; } = string.Empty;
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}
