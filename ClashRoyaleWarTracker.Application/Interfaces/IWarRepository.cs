using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IWarRepository
    {
        Task<bool> AddPlayerWarHistoriesAsync(List<PlayerWarHistory> playerWarHistories);
    }
}
