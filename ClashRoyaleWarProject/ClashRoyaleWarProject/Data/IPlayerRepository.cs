using ClashRoyaleProject.Shared.Models;

namespace ClashRoyaleWarProject.Data
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerByTagAsync(string tag);
        Task<IEnumerable<Player>> GetAllPlayersAsync();
        Task AddOrUpdatePlayerAsync(Player player);
        Task DeletePlayerAsync(string tag);
    }
}