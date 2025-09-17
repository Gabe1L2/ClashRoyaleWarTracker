using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IWarRepository
    {
        Task<bool> AddPlayerWarHistoriesAsync(List<PlayerWarHistory> playerWarHistories);
        Task<List<PlayerWarHistory>?> GetPlayerWarHistoriesAsync(Player player, int numOfWeeksToUse, bool aboveFiveThousandTrophies);
        Task<List<PlayerWarHistoryExpanded>> GetAllPlayerWarHistoriesExpandedAsync(bool is5k);

    }
}
