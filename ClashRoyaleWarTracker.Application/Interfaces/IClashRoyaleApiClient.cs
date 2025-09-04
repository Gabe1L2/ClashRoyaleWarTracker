using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IClashRoyaleApiClient
    {
        Task<Clan?> GetClanByTagAsync(string clanTag);
        Task<RiverRaceLogResponse?> GetRiverRaceLogAsync(string clanTag);
    }
}