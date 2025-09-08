using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class WarRepository : IWarRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WarRepository> _logger;
        public WarRepository(ApplicationDbContext context, ILogger<WarRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddPlayerWarHistoriesAsync(List<PlayerWarHistory> playerWarHistories)
        {
            try
            {
                _logger.LogDebug("Adding {Count} player war histories to the database", playerWarHistories.Count);
                int successCount = 0;
                int duplicates = 0;
                foreach (var playerWarHistory in playerWarHistories)
                {
                    var exists = await _context.PlayerWarHistories.AnyAsync(rwh =>
                        rwh.PlayerID == playerWarHistory.PlayerID &&
                        rwh.ClanHistoryID == playerWarHistory.ClanHistoryID);
                    if (!exists)
                    {
                        playerWarHistory.LastUpdated = DateTime.Now;
                        _logger.LogDebug($"Adding player war history for PlayerID {playerWarHistory.PlayerID} and ClanHistoryID {playerWarHistory.ClanHistoryID}");
                        await _context.PlayerWarHistories.AddAsync(playerWarHistory);
                        successCount++;
                    }
                    else
                    {
                        _logger.LogDebug($"Player war history for PlayerID {playerWarHistory.PlayerID} and ClanHistoryID {playerWarHistory.ClanHistoryID} already exists. Skipping.");
                        duplicates++;
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully added {successCount} player war histories to the database. {duplicates} duplicates skipped.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add raw war histories to the database");
                throw new InvalidOperationException("Failed to add raw war histories to the database", ex);
            }
        }
    }
}
