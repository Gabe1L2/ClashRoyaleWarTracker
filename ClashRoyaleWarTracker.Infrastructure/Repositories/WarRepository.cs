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

        public async Task<bool> AddRawWarHistoriesAsync(List<RawWarHistory> rawWarHistories)
        {
            try
            {
                foreach (var rawWarHistory in rawWarHistories)
                {
                    var exists = await _context.RawWarData.AnyAsync(rwh =>
                        rwh.PlayerID == rawWarHistory.PlayerID &&
                        rwh.ClanHistoryID == rawWarHistory.ClanHistoryID);
                    if (!exists)
                    {
                        rawWarHistory.InsertDate = DateTime.Now;
                        await _context.RawWarData.AddAsync(rawWarHistory);
                        await _context.SaveChangesAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add raw war histories to the database", ex);
            }
        }
    }
}
