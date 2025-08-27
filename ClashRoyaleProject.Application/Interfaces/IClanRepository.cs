using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Application.Interfaces
{
    public interface IClanRepository
    {
        Task AddOrUpdateClanAsync(Clan clan);
    }
}