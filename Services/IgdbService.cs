using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BacklogBasement.Services
{
    public class IgdbService : IIgdbService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IgdbService> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _accessToken;
        private DateTime _tokenExpiry;

        public IgdbService(HttpClient httpClient, IConfiguration configuration, ILogger<IgdbService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _clientId = configuration["Igdb:ClientId"] ?? throw new ArgumentNullException("Igdb:ClientId");
            _clientSecret = configuration["Igdb:ClientSecret"] ?? throw new ArgumentNullException("Igdb:ClientSecret");
        }

        public async Task<IEnumerable<IgdbGame>> SearchGamesAsync(string query)
        {
            await EnsureAccessTokenAsync();

            var searchQuery = $@"
                fields id, name, summary, first_release_date, cover.image_id;
                search ""{query}"";
                limit 20;
            ";

            try 
            {
                var response = await PostAsync<IgdbGame[]>("games", searchQuery);
                var gameData = response ?? Array.Empty<IgdbGame>();
                _logger?.LogInformation("IGDB search returned {Count} results for query: {Query}", 
                    gameData.Length, query);
                return gameData;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger?.LogError("IGDB API returned 401 - check Twitch application permissions for IGDB access");
                return Array.Empty<IgdbGame>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "IGDB search failed for query: {Query}", query);
                return Array.Empty<IgdbGame>();
            }
        }

        public async Task<IgdbGame?> GetGameAsync(long igdbId)
        {
            await EnsureAccessTokenAsync();

            var query = $@"
                fields id, name, summary, first_release_date, cover.image_id;
                where id = {igdbId};
            ";

            var games = await PostAsync<IgdbGame[]>("games", query);
            return games?.FirstOrDefault();
        }

        private async Task EnsureAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow.AddMinutes(5))
                return;

            try
            {
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "analytics:read:games")
                });

                _logger?.LogInformation("Requesting Twitch OAuth token with scope: analytics:read:games");
                var response = await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", requestBody);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogError("Twitch OAuth failed with {StatusCode}: {Content}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"Twitch OAuth failed: {response.StatusCode}", null, response.StatusCode);
                }

                _logger?.LogInformation("Twitch OAuth response (first 100 chars): {ResponseContent}", responseContent.Substring(0, Math.Min(100, responseContent.Length)));
                
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger?.LogInformation("Token deserialization result: {Success}, Token exists: {HasToken}", 
                    tokenResponse != null, tokenResponse?.AccessToken != null);

                if (tokenResponse != null)
                {
                    _accessToken = tokenResponse.AccessToken;
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                    _logger?.LogInformation("Successfully obtained access token: {TokenPrefix}... (expires: {Expiry})",
                        _accessToken?.Substring(0, Math.Min(10, _accessToken.Length)), _tokenExpiry);
                }
                else
                {
                    _logger?.LogError("Token deserialization returned null - JSON structure might be unexpected");
                    throw new InvalidOperationException("Failed to deserialize OAuth token response");
                }
            }
            catch (JsonException jsonEx)
            {
                _logger?.LogError(jsonEx, "JSON deserialization failed - possible API format change");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to obtain access token from Twitch");
                throw;
            }
        }

        private async Task<T?> PostAsync<T>(string endpoint, string query)
        {
            await EnsureAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.igdb.com/v4/{endpoint}")
            {
                Content = new StringContent(query, Encoding.UTF8, "text/plain")
            };

            request.Headers.Add("Client-ID", _clientId);
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger?.LogWarning("IGDB API unauthorized. Ensure your Twitch application has IGDB access enabled in Twitch Console");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to query IGDB API");
                throw;
            }
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;
            
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
            
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;
            
            [JsonPropertyName("scope")]
            public string[] Scope { get; set; } = Array.Empty<string>();
        }
    }
}