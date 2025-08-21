using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using System.Net.Http.Json;

namespace ClashRoyaleProject.Infrastructure.Http
{
    public class ClashRoyaleApiClient : IClashRoyaleApiClient
    {
        private readonly HttpClient _httpClient;

        public ClashRoyaleApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Player?> GetPlayerByTagAsync(string playerTag)
        {
            var response = await _httpClient.GetAsync($"players/{playerTag}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Player>();
            }
            return null; // Handle error or throw exception as needed
        }
        // Add more methods as needed for clan, war, etc.
    }
}
