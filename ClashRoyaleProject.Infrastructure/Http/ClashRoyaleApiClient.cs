using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using ClashRoyaleProject.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ClashRoyaleProject.Infrastructure.Http
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

        public async Task<Clan> GetClanByTagAsync(string clanTag)
        {
            try
            {
                _logger.LogInformation("Making API request for clan {ClanTag}", clanTag);

                // Construct the full URI manually to ensure it's correct
                var baseAddress = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://api.clashroyale.com/v1";
                var fullUri = $"{baseAddress}/clans/%23{clanTag}";

                _logger.LogInformation("Full URI: {FullUri}", fullUri);

                var response = await _httpClient.GetAsync(fullUri);

                if (response.IsSuccessStatusCode)
                {
                    var clanData = await response.Content.ReadFromJsonAsync<ClashRoyaleApiClan>();

                    if (clanData != null)
                    {
                        _logger.LogInformation("Successfully retrieved clan {ClanName} with tag {ClanTag}", clanData.Name, clanData.Tag);

                        // Map from API response to the domain model
                        return new Clan
                        {
                            Tag = clanData.Tag,
                            Name = clanData.Name,
                            WarTrophies = clanData.ClanWarTrophies,
                        };
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API request failed for clan {ClanTag} with status {StatusCode}. Response: {ResponseContent}", clanTag, response.StatusCode, responseContent);
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
                _logger.LogError(ex, "Unexpected error when fetching clan {ClanTag}", clanTag);
                throw;
            }
        }
    }
}