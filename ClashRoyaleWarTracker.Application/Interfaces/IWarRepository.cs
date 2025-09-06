using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IWarRepository
    {
        Task<bool> AddRawWarHistoriesAsync(List<RawWarHistory> rawWarHistories);
    }
}
