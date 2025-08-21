using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Application.Interfaces
{
    public interface IRepository
    {
        Task<Player?> GetPlayerByTagAsync(string tag);
        Task<IEnumerable<Player>> GetAllPlayersAsync();
        Task AddOrUpdatePlayerAsync(Player player);
        Task DeletePlayerAsync(string tag);
    }
}