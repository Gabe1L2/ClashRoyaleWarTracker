using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Application.Interfaces
{
    public interface IClashRoyaleApiClient
    {
        Task<Player?> GetPlayerByTagAsync(string playerTag);
        // Add more methods as needed for clan, war, etc.
    }
}