using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Application.Interfaces
{
    public interface IClanRepository
    {
        Task<bool> AddClanAsync(Clan clan);
        Task<Clan?> GetClanAsync(string clanTag);
        Task<IEnumerable<Clan>> GetAllClansAsync();
        Task<bool> DeleteClanAsync(string clanTag);
        Task<bool> UpdateClanAsync(Clan clan);
        Task<bool> UpdateClanHistoryAsync(Clan clan, List<ClanHistory> clanHistories);
    }
}