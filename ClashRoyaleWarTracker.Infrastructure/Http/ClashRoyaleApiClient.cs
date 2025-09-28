using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace ClashRoyaleWarTracker.Infrastructure.Http
{
    public class ClashRoyaleApiClient : IClashRoyaleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClashRoyaleApiClient> _logger;

        public ClashRoyaleApiClient(HttpClient httpClient, ILogger<ClashRoyaleApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Clan?> GetClanByTagAsync(string clanTag)
        {
            try
            {
                _logger.LogInformation("Making API request for clan {ClanTag}", clanTag);

                var response = await _httpClient.GetAsync($"clans/%23{clanTag}");

                if (response.IsSuccessStatusCode)
                {
                    var clanData = await response.Content.ReadFromJsonAsync<ClashRoyaleApiClan>();

                    if (clanData != null && !string.IsNullOrEmpty(clanData.Tag) && !string.IsNullOrEmpty(clanData.Name))
                    {
                        _logger.LogInformation("Successfully retrieved clan {ClanName} with tag {ClanTag}", clanData.Name, clanData.Tag);

                        return new Clan
                        {
                            Tag = clanData.Tag,
                            Name = clanData.Name,
                            WarTrophies = clanData.ClanWarTrophies,
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Clan data is null or missing required fields for tag {ClanTag}", clanTag);
                        throw new InvalidOperationException($"Clan data is null or missing required fields for tag {clanTag}");
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API request failed for clan {ClanTag} with status {StatusCode}. Response: {ResponseContent}", 
                        clanTag, response.StatusCode, responseContent);
                    throw new Exception($"API request failed with status {response.StatusCode}");
                }

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when fetching clan {ClanTag}", clanTag);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching clan {ClanTag}", clanTag);
                throw;
            }
        }

        public async Task<RiverRaceLogResponse?> GetRiverRaceLogAsync(string clanTag)
        {
            try
            {
                _logger.LogInformation("Making API request for river race logs for clan {ClanTag}", clanTag);

                var response = await _httpClient.GetAsync($"clans/%23{clanTag}/riverracelog");

                if (response.IsSuccessStatusCode)
                {
                    var riverRaceLog = await response.Content.ReadFromJsonAsync<RiverRaceLogResponse>();
                    if (riverRaceLog != null)
                    {
                        _logger.LogInformation("Successfully retrieved river race logs for clan {ClanTag}", clanTag);
                        return riverRaceLog;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API request failed for river race logs for clan {ClanTag} with status {StatusCode}. Response: {ResponseContent}",
                        clanTag, response.StatusCode, responseContent);
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when fetching clan {ClanTag}", clanTag);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching river race logs for clan {ClanTag}", clanTag);
                throw;
            }
        }

        public async Task<ClashRoyalePlayerInfo?> GetPlayerByTagAsync(string playerTag)
        {
            try
            {
                _logger.LogInformation("Making API request for player {PlayerTag}", playerTag);

                var response = await _httpClient.GetAsync($"players/%23{playerTag}");

                if (response.IsSuccessStatusCode)
                {
                    var playerData = await response.Content.ReadFromJsonAsync<ClashRoyaleApiPlayer>();

                    if (playerData != null && !string.IsNullOrEmpty(playerData.Tag))
                    {
                        _logger.LogInformation("Successfully retrieved player {PlayerName} with tag {PlayerTag}", playerData.Name, playerData.Tag);

                        return new ClashRoyalePlayerInfo
                        {
                            Tag = Regex.Replace(playerData.Tag, @"[^a-zA-Z0-9]", ""),
                            Name = playerData.Name,
                            CurrentClanTag = playerData.Clan != null ? Regex.Replace(playerData.Clan.Tag, @"[^a-zA-Z0-9]", "") : null,
                            CurrentClanName = playerData.Clan?.Name
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Player data is null or missing required fields for tag {PlayerTag}", playerTag);
                        return null;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API request failed for player {PlayerTag} with status {StatusCode}. Response: {ResponseContent}",
                        playerTag, response.StatusCode, responseContent);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when fetching player {PlayerTag}", playerTag);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching player {PlayerTag}", playerTag);
                return null;
            }
        }
    }
}