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
                fields id, name, summary, first_release_date, aggregated_rating, cover.image_id;
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

        public async Task<Dictionary<string, IgdbGame>> BatchSearchGamesAsync(IEnumerable<string> names)
        {
            var nameList = names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (!nameList.Any()) return new Dictionary<string, IgdbGame>(StringComparer.OrdinalIgnoreCase);

            await EnsureAccessTokenAsync();

            var resultMap = new Dictionary<string, IgdbGame>(StringComparer.OrdinalIgnoreCase);

            // Bundle 10 sub-queries into one multiquery HTTP request.
            // search "name" inside query games is silently ignored by IGDB — only where clauses
            // actually filter results. Use where name = "..." which IGDB evaluates case-insensitively.
            const int batchSize = 10;

            for (int i = 0; i < nameList.Count; i += batchSize)
            {
                var batch = nameList.Skip(i).Take(batchSize).ToList();
                var multiQuery = new System.Text.StringBuilder();

                for (int j = 0; j < batch.Count; j++)
                {
                    var escaped = batch[j].Replace("\\", "\\\\").Replace("\"", "\\\"");
                    multiQuery.AppendLine($"query games \"{j}\" {{");
                    multiQuery.AppendLine($"  fields id, name, summary, first_release_date, aggregated_rating, cover.image_id;");
                    multiQuery.AppendLine($"  where name = \"{escaped}\";");
                    multiQuery.AppendLine($"  limit 1;");
                    multiQuery.AppendLine("};");
                }

                _logger?.LogInformation("IGDB multiquery batch {Index}, query:\n{Query}", i, multiQuery.ToString());
                try
                {
                    var rawResponse = await PostRawAsync("multiquery", multiQuery.ToString());
                    _logger?.LogInformation("IGDB multiquery batch {Index} raw response: {Response}", i, rawResponse?[..Math.Min(500, rawResponse?.Length ?? 0)]);

                    var response = rawResponse == null ? null :
                        System.Text.Json.JsonSerializer.Deserialize<MultiQueryResult[]>(rawResponse, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger?.LogInformation("IGDB multiquery batch {Index}: deserialized {Count} results", i, response?.Length ?? -1);

                    if (response != null)
                    {
                        foreach (var queryResult in response)
                        {
                            _logger?.LogInformation("  SubQuery '{Name}': count={Count}, results={ResultCount}", queryResult.Name, queryResult.Count, queryResult.Result?.Length ?? 0);
                            if (int.TryParse(queryResult.Name, out var idx) && idx < batch.Count
                                && queryResult.Result != null && queryResult.Result.Length > 0)
                            {
                                var igdbGame = queryResult.Result[0];
                                if (NamesMatch(batch[idx], igdbGame.Name))
                                    resultMap[batch[idx]] = igdbGame;
                                else
                                    _logger?.LogInformation("IGDB name mismatch: searched '{Input}', got '{Result}' — skipping", batch[idx], igdbGame.Name);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "IGDB multiquery EXCEPTION for batch starting at index {Index}", i);
                }

                if (i + batchSize < nameList.Count)
                    await Task.Delay(300);
            }

            // === Second pass: try prefix/suffix variants for unmatched names with ": " ===
            // e.g. "Wave Race 64: Kawasaki Jet Ski" → try "Wave Race 64"
            //      "RR64: Ridge Racer 64"           → try "Ridge Racer 64"
            var unmatchedAfterFirst = nameList.Where(n => !resultMap.ContainsKey(n)).ToList();
            var altSearchMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in unmatchedAfterFirst)
            {
                var colonIdx = name.IndexOf(": ");
                if (colonIdx > 0)
                {
                    var prefix = name.Substring(0, colonIdx).Trim();
                    var suffix = name.Substring(colonIdx + 2).Trim();
                    if (prefix.Length >= 6) altSearchMap.TryAdd(prefix, name);
                    if (suffix.Length >= 3) altSearchMap.TryAdd(suffix, name);
                }
            }

            if (altSearchMap.Any())
            {
                var altNames = altSearchMap.Keys.ToList();
                for (int i = 0; i < altNames.Count; i += batchSize)
                {
                    var batch = altNames.Skip(i).Take(batchSize).ToList();
                    var multiQuery = new System.Text.StringBuilder();
                    for (int j = 0; j < batch.Count; j++)
                    {
                        var escaped = batch[j].Replace("\\", "\\\\").Replace("\"", "\\\"");
                        multiQuery.AppendLine($"query games \"{j}\" {{");
                        multiQuery.AppendLine($"  fields id, name, summary, first_release_date, aggregated_rating, cover.image_id;");
                        multiQuery.AppendLine($"  where name = \"{escaped}\";");
                        multiQuery.AppendLine($"  limit 1;");
                        multiQuery.AppendLine("};");
                    }
                    try
                    {
                        var rawResponse = await PostRawAsync("multiquery", multiQuery.ToString());
                        var response = rawResponse == null ? null :
                            System.Text.Json.JsonSerializer.Deserialize<MultiQueryResult[]>(rawResponse, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (response != null)
                        {
                            foreach (var queryResult in response)
                            {
                                if (int.TryParse(queryResult.Name, out var idx) && idx < batch.Count
                                    && queryResult.Result != null && queryResult.Result.Length > 0)
                                {
                                    var originalName = altSearchMap[batch[idx]];
                                    var igdbGame = queryResult.Result[0];
                                    if (!resultMap.ContainsKey(originalName) && NamesMatch(originalName, igdbGame.Name))
                                        resultMap[originalName] = igdbGame;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "IGDB alt-query exception for batch at index {Index}", i);
                    }
                    if (i + batchSize < altNames.Count)
                        await Task.Delay(300);
                }
            }

            // === Third pass: search fallback for still-unmatched ===
            // Handles cases where the name format differs (e.g. "Rampage: World Tour" vs "Rampage World Tour")
            var stillUnmatched = nameList.Where(n => !resultMap.ContainsKey(n)).ToList();
            foreach (var name in stillUnmatched)
            {
                try
                {
                    var escaped = name.Replace("\\", "\\\\").Replace("\"", "\\\"");
                    var query = $"fields id, name, summary, first_release_date, aggregated_rating, cover.image_id; search \"{escaped}\"; limit 3;";
                    var results = await PostAsync<IgdbGame[]>("games", query);
                    if (results != null)
                    {
                        var best = results.FirstOrDefault(g => NamesMatch(name, g.Name));
                        if (best != null)
                            resultMap[name] = best;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "IGDB search fallback exception for '{Name}'", name);
                }
            }

            return resultMap;
        }

        private class MultiQueryResult
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("count")]
            public int Count { get; set; }

            [JsonPropertyName("result")]
            public IgdbGame[]? Result { get; set; }
        }

        /// <summary>
        /// Returns true when the IGDB-returned name is close enough to the searched input.
        /// Handles: case differences, leading articles (The/A/An), punctuation, and
        /// off-by-one characters (e.g. "Blackthorn" vs "Blackthorne").
        /// </summary>
        private static bool NamesMatch(string input, string igdbName)
        {
            if (string.Equals(input, igdbName, StringComparison.OrdinalIgnoreCase))
                return true;

            var a = NormalizeForComparison(input);
            var b = NormalizeForComparison(igdbName);

            if (a == b) return true;

            // Allow one character difference (handles trailing 'e', apostrophes, etc.)
            if (Math.Abs(a.Length - b.Length) <= 2 && LevenshteinDistance(a, b) <= 2)
                return true;

            // Allow one being a prefix of the other (subtitles stripped differently)
            if (a.StartsWith(b) || b.StartsWith(a))
                return true;

            // Allow IGDB name to be a significant substring of input (e.g. "RR64: Ridge Racer 64" → "Ridge Racer 64")
            if (b.Length >= 8 && a.Contains(b))
                return true;

            return false;
        }

        private static string NormalizeForComparison(string s)
        {
            // Lowercase
            var n = s.ToLowerInvariant();
            // Strip leading articles
            n = System.Text.RegularExpressions.Regex.Replace(n, @"^(the|a|an)\s+", "");
            // Remove punctuation (keep alphanumeric and spaces)
            n = System.Text.RegularExpressions.Regex.Replace(n, @"[^a-z0-9 ]", "");
            // Collapse whitespace
            n = System.Text.RegularExpressions.Regex.Replace(n, @"\s+", " ").Trim();
            return n;
        }

        private static int LevenshteinDistance(string a, string b)
        {
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            return d[a.Length, b.Length];
        }

        public async Task<IgdbGame?> GetGameAsync(long igdbId)
        {
            await EnsureAccessTokenAsync();

            var query = $@"
                fields id, name, summary, first_release_date, aggregated_rating, cover.image_id;
                where id = {igdbId};
            ";

            var games = await PostAsync<IgdbGame[]>("games", query);
            return games?.FirstOrDefault();
        }

        public async Task<long?> FindIgdbIdBySteamIdAsync(long steamAppId)
        {
            try
            {
                await EnsureAccessTokenAsync();
                var query = $@"fields game; where uid = ""{steamAppId}"" & category = 1; limit 1;";
                var results = await PostAsync<ExternalGame[]>("external_games", query);
                if (results?.Length > 0) return results[0].Game;
                // Fallback: some IGDB entries are missing the category field
                query = $@"fields game; where uid = ""{steamAppId}""; limit 1;";
                var fallback = await PostAsync<ExternalGame[]>("external_games", query);
                return fallback?.FirstOrDefault()?.Game;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to find IGDB ID for Steam App ID {SteamAppId}", steamAppId);
                return null;
            }
        }

        private class ExternalGame
        {
            [JsonPropertyName("game")]
            public long Game { get; set; }
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

        private async Task<string?> PostRawAsync(string endpoint, string query)
        {
            await EnsureAccessTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.igdb.com/v4/{endpoint}")
            {
                Content = new StringContent(query, Encoding.UTF8, "text/plain")
            };
            request.Headers.Add("Client-ID", _clientId);
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            _logger?.LogInformation("IGDB raw HTTP status: {Status}", response.StatusCode);
            return content;
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