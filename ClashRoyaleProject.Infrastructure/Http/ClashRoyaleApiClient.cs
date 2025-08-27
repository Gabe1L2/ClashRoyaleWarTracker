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

                var response = await _httpClient.GetAsync($"clans/%23{clanTag}");

                if (response.IsSuccessStatusCode)
                {
                    var clanData = await response.Content.ReadFromJsonAsync<ClashRoyaleApiClan>();

                    if (clanData != null)
                    {
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
                    _logger.LogWarning("API request failed for clan {ClanTag} with status {StatusCode}", clanTag, response.StatusCode);
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
