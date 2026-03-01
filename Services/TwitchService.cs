using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
        private readonly ConcurrentDictionary<long, long?> _igdbToTwitchIdCache = new();

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

                var twitchGameId = await GetTwitchGameIdAsync(igdbId);
                if (twitchGameId == null) return [];

                var url = $"https://api.twitch.tv/helix/streams?game_id={twitchGameId}&first={limit}";
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

        public async Task<TwitchLiveDto> GetLiveStreamAsync(string twitchUserId)
        {
            try
            {
                await EnsureAccessTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api.twitch.tv/helix/streams?user_id={twitchUserId}&first=1");
                request.Headers.Add("Client-ID", _clientId);
                request.Headers.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                var result = JsonSerializer.Deserialize<TwitchStreamsResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var stream = result?.Data?.FirstOrDefault(s => s.Type == "live");
                if (stream == null)
                    return new TwitchLiveDto { IsLive = false };

                long? igdbGameId = long.TryParse(stream.GameId, out var gid) && gid > 0 ? gid : null;

                return new TwitchLiveDto
                {
                    IsLive = true,
                    StreamTitle = stream.Title,
                    GameName = stream.GameName,
                    IgdbGameId = igdbGameId,
                    ViewerCount = stream.ViewerCount,
                    TwitchLogin = stream.UserLogin,
                    ThumbnailUrl = stream.ThumbnailUrl
                        ?.Replace("{width}", "440")
                        .Replace("{height}", "248")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch live status for Twitch user {TwitchUserId}", twitchUserId);
                return new TwitchLiveDto { IsLive = false };
            }
        }

        public async Task<List<TwitchStreamedGame>> GetStreamedGamesAsync(string twitchUserId)
        {
            try
            {
                await EnsureAccessTokenAsync();

                var gameTotals = new Dictionary<string, (string GameName, int TotalMinutes)>();
                string? cursor = null;
                const int maxPages = 5;

                for (int page = 0; page < maxPages; page++)
                {
                    var url = $"https://api.twitch.tv/helix/videos?user_id={twitchUserId}&type=archive&first=100";
                    if (cursor != null) url += $"&after={cursor}";

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Client-ID", _clientId);
                    request.Headers.Add("Authorization", $"Bearer {_accessToken}");

                    var response = await _httpClient.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode) break;

                    var result = JsonSerializer.Deserialize<TwitchVideosResponse>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Data == null || result.Data.Count == 0) break;

                    foreach (var video in result.Data)
                    {
                        if (string.IsNullOrEmpty(video.GameId) || video.GameId == "0") continue;
                        var minutes = ParseTwitchDuration(video.Duration);
                        if (gameTotals.TryGetValue(video.GameId, out var existing))
                            gameTotals[video.GameId] = (existing.GameName, existing.TotalMinutes + minutes);
                        else
                            gameTotals[video.GameId] = (video.GameName, minutes);
                    }

                    cursor = result.Pagination?.Cursor;
                    if (cursor == null) break;
                }

                return gameTotals
                    .Where(kvp => long.TryParse(kvp.Key, out var id) && id > 0)
                    .Select(kvp => new TwitchStreamedGame(
                        long.Parse(kvp.Key),
                        kvp.Value.GameName,
                        kvp.Value.TotalMinutes))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch streamed games for Twitch user {TwitchUserId}", twitchUserId);
                return [];
            }
        }

        private static int ParseTwitchDuration(string? duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;
            int hours = 0, minutes = 0;
            var hMatch = Regex.Match(duration, @"(\d+)h");
            var mMatch = Regex.Match(duration, @"(\d+)m");
            if (hMatch.Success) hours = int.Parse(hMatch.Groups[1].Value);
            if (mMatch.Success) minutes = int.Parse(mMatch.Groups[1].Value);
            return hours * 60 + minutes;
        }

        private async Task<long?> GetTwitchGameIdAsync(long igdbId)
        {
            if (_igdbToTwitchIdCache.TryGetValue(igdbId, out var cached))
                return cached;

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.twitch.tv/helix/games?igdb_id={igdbId}");
            request.Headers.Add("Client-ID", _clientId);
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var result = JsonSerializer.Deserialize<TwitchGamesResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var twitchId = result?.Data?.FirstOrDefault()?.Id is string idStr && long.TryParse(idStr, out var id)
                ? id : (long?)null;

            _igdbToTwitchIdCache[igdbId] = twitchId;
            return twitchId;
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

        private class TwitchGamesResponse
        {
            [JsonPropertyName("data")]
            public List<TwitchGameData> Data { get; set; } = new();
        }

        private class TwitchGameData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
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
            [JsonPropertyName("game_id")]
            public string GameId { get; set; } = string.Empty;
            [JsonPropertyName("game_name")]
            public string GameName { get; set; } = string.Empty;
            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;
            [JsonPropertyName("viewer_count")]
            public int ViewerCount { get; set; }
            [JsonPropertyName("thumbnail_url")]
            public string ThumbnailUrl { get; set; } = string.Empty;
        }

        private class TwitchVideosResponse
        {
            [JsonPropertyName("data")]
            public List<TwitchVideoData> Data { get; set; } = new();
            [JsonPropertyName("pagination")]
            public TwitchPagination? Pagination { get; set; }
        }

        private class TwitchVideoData
        {
            [JsonPropertyName("game_id")]
            public string GameId { get; set; } = string.Empty;
            [JsonPropertyName("game_name")]
            public string GameName { get; set; } = string.Empty;
            [JsonPropertyName("duration")]
            public string Duration { get; set; } = string.Empty;
        }

        private class TwitchPagination
        {
            [JsonPropertyName("cursor")]
            public string? Cursor { get; set; }
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
