using ClashRoyaleProject.Shared.Models;

namespace ClashRoyaleWarProject.Services
{
    public interface IClashRoyaleApiService
    {
        Task<Player?> GetPlayerByTagAsync(string playerTag);
        // Add more methods as needed for clan, war, etc.
    }
}