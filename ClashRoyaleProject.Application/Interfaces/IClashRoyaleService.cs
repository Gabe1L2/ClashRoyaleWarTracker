using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Application.Interfaces
{
    public interface IClashRoyaleService
    {
        Task<Clan> GetClanByTagAsync(string clanTag);
    }
}