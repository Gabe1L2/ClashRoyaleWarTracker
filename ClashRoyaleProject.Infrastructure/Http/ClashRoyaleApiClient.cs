using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using ClashRoyaleProject.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ClashRoyaleProject.Infrastructure.Http
{
    public class ClashRoyaleApiClient : IClashRoyaleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClashRoyaleApiClient> _logger;
        private readonly IConfiguration _configuration;

        public ClashRoyaleApiClient(HttpClient httpClient, ILogger<ClashRoyaleApiClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Clan> GetClanByTagAsync(string clanTag)
        {
            try
            {
                _logger.LogInformation("Making API request for clan {ClanTag}", clanTag);

                // Get API key from configuration
                //var apiKey = _configuration["ClashRoyaleApi:ApiKey"];
                var apiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiIsImtpZCI6IjI4YTMxOGY3LTAwMDAtYTFlYi03ZmExLTJjNzQzM2M2Y2NhNSJ9.eyJpc3MiOiJzdXBlcmNlbGwiLCJhdWQiOiJzdXBlcmNlbGw6Z2FtZWFwaSIsImp0aSI6IjQ4NDQ2OGIzLTRlNjYtNGQ4Ni1hNDAwLWQ0ZTYwZjZhYzQxZCIsImlhdCI6MTc1NjI0MjQyOSwic3ViIjoiZGV2ZWxvcGVyLzQ2MTBkZjdiLTA3MGYtNTk0ZC05NmI5LTYzNjlkZjQ5ZGNlNSIsInNjb3BlcyI6WyJyb3lhbGUiXSwibGltaXRzIjpbeyJ0aWVyIjoiZGV2ZWxvcGVyL3NpbHZlciIsInR5cGUiOiJ0aHJvdHRsaW5nIn0seyJjaWRycyI6WyIxMzguMjQ3LjM4LjM4Il0sInR5cGUiOiJjbGllbnQifV19.Z6NQdVRKxDfXqBZNwubCg-prSKPTtssFWF_SA_yx9C2sZN9ErJXyoYaY7bRJsT9kVjEt1agc-EQSpEVoIaUgFA";

                // Create request with manual headers (the approach that worked)
                using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.clashroyale.com/v1/clans/%23{clanTag}");
                
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Headers.Add("Accept", "application/json");
                
                _logger.LogInformation("Request URI: {RequestUri}", request.RequestUri);

                var response = await _httpClient.SendAsync(request);

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