using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerAsync(string playerTag);
        Task<int> AddPlayerAsync(Player player);
    }
}
