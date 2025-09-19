using ClashRoyaleWarTracker.Application.Models;
using System.Numerics;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerAsync(string playerTag);
        Task<Player?> GetPlayerByIdAsync(int playerId);
        Task<int> AddPlayerAsync(Player player);
        Task<List<Player>> GetAllActivePlayersAsync();
        Task<bool> UpdatePlayerStatusAsync(int playerId, string status);
        Task UpsertPlayerAverageAsync(PlayerAverage playerAverage);
        Task<List<PlayerAverageDTO>> GetAllPlayerAveragesAsync();
    }
}
