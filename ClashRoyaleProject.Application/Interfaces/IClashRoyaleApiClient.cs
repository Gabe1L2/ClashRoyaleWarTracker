using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Application.Interfaces
{
    public interface IClashRoyaleApiClient
    {
        Task<Clan?> GetClanByTagAsync(string clanTag);
        Task<RiverRaceLogResponse?> GetRiverRaceLogAsync(string clanTag);
    }
}