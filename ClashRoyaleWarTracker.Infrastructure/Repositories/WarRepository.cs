using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class WarRepository : IWarRepository
    {
        private readonly ApplicationDbContext _context;
        public WarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddPlayerWarHistoriesAsync(List<PlayerWarHistory> playerWarHistories)
        {
            try
            {
                foreach (var playerWarHistory in playerWarHistories)
                {
                    var exists = await _context.PlayerWarHistories.AnyAsync(rwh =>
                        rwh.PlayerID == playerWarHistory.PlayerID &&
                        rwh.ClanHistoryID == playerWarHistory.ClanHistoryID);
                    if (!exists)
                    {
                        playerWarHistory.LastUpdated = DateTime.Now;
                        await _context.PlayerWarHistories.AddAsync(playerWarHistory);
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add raw war histories to the database", ex);
            }
        }
    }
}
