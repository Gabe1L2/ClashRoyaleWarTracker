using ClashRoyaleWarTracker.Application.Models;
using System.Numerics;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerAsync(string playerTag);
        Task<int> AddPlayerAsync(Player player);
        Task<List<Player>> GetAllActivePlayersAsync();
        Task UpsertPlayerAverageAsync(PlayerAverage playerAverage);
        Task<List<PlayerAverageDTO>> GetAllPlayerAveragesAsync();
    }
}
