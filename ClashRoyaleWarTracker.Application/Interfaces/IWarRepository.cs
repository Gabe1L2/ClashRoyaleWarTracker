using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IWarRepository
    {
        Task<bool> AddPlayerWarHistoriesAsync(List<PlayerWarHistory> playerWarHistories);
        /// <summary>
        /// Retrieves the player's war histories from their most recent weeks of participation, filtered by trophy level and defensive play weeks.
        /// </summary>
        /// <returns>
        /// A list of PlayerWarHistory records from the player's most recent war weeks, or null if no records found.
        /// Only includes weeks where at least one player had 0 boat attacks (defensive play weeks).
        /// </returns>
        Task<List<PlayerWarHistory>?> GetPlayerWarHistoriesAsync(Player player, int numOfWeeksToUse, bool aboveFiveThousandTrophies);
    }
}
