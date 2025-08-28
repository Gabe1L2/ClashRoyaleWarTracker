using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Application.Interfaces
{
    public interface IClanRepository
    {
        Task<bool> AddClanAsync(Clan clan);
        Task<IEnumerable<Clan>> GetAllClansAsync();
        Task<bool> DeleteClanAsync(string clanTag);
        Task<bool> UpdateClanAsync(Clan clan);
    }
}