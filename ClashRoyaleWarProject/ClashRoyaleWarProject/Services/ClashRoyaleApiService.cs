using ClashRoyaleProject.Shared.Models;
using ClashRoyaleWarProject.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ClashRoyaleWarProject.Services
{
    public class ClashRoyaleApiService : IClashRoyaleApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ClashRoyaleApiConfig _config;

        public ClashRoyaleApiService(HttpClient httpClient, ClashRoyaleApiConfig config)
        {
            _httpClient = httpClient;
            _config = config;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        }

        public async Task<Player?> GetPlayerByTagAsync(string playerTag)
        {
            // Example endpoint: /players/%23TAG
            var tag = playerTag.StartsWith("#") ? playerTag.Replace("#", "%23") : "%23" + playerTag;
            var response = await _httpClient.GetAsync($"/players/{tag}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            // TODO: Map JSON to Player model
            // return JsonSerializer.Deserialize<Player>(json);
            return null;
        }
    }
}