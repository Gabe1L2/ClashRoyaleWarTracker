using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IClanRepository
    {
        Task<bool> AddClanAsync(Clan clan);
        Task<Clan?> GetClanAsync(string clanTag);
        Task<IEnumerable<Clan>> GetAllClansAsync();
        Task<bool> DeleteClanAsync(string clanTag);
        Task<bool> UpdateClanAsync(Clan clan);
        Task<bool> PopulateClanHistoryAsync(Clan clan, List<ClanHistory> clanHistories);
        Task<ClanHistory?> GetClanHistoryAsync(int clanID, int seasonID, int weekIndex);
        Task<int> GetMostRecentClanIDAsync(Player player, bool aboveFiveThousandTrophies);

    }
}