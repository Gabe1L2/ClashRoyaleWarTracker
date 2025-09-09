using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;

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

        /// <summary>
        /// Retrieves the player's war histories from their most recent weeks of participation, filtered by trophy level and defensive play weeks.
        /// </summary>
        /// <returns>
        /// A list of PlayerWarHistory records from the player's most recent war weeks, or null if no records found.
        /// Only includes weeks where at least one player had 0 boat attacks (defensive play weeks).
        /// </returns>
        public async Task<List<PlayerWarHistory>?> GetPlayerWarHistoriesAsync(Player player, int numOfWeeksToUse, bool aboveFiveThousandTrophies)
        {
            try
            {
                _logger.LogDebug($"Retrieving last {numOfWeeksToUse} weeks of war for {player.Name} - {player.Tag} {(aboveFiveThousandTrophies ? "above/equal to" : "below")} 5000 war trophies");

                var trophyCondition = aboveFiveThousandTrophies ? ">= 5000" : "< 5000";

                var sql = $@"
                        WITH PlayerWarWeeks AS (
                            -- Get all weeks where this specific player participated in war
                            SELECT DISTINCT TOP (@numWeeks) ch.SeasonID, ch.WeekIndex
                            FROM PlayerWarHistories pwh
                            INNER JOIN ClanHistories ch ON pwh.ClanHistoryID = ch.ID
                            WHERE pwh.PlayerID = @playerId
                                AND ch.WarTrophies {trophyCondition}
                                AND EXISTS (
                                    SELECT 1 
                                    FROM PlayerWarHistories pwh_check
                                    INNER JOIN ClanHistories ch_check ON pwh_check.ClanHistoryID = ch_check.ID
                                    WHERE ch_check.SeasonID = ch.SeasonID 
                                        AND ch_check.WeekIndex = ch.WeekIndex
                                        AND ch_check.WarTrophies {trophyCondition}
                                        AND pwh_check.BoatAttacks = 0
                                )
                            ORDER BY ch.SeasonID DESC, ch.WeekIndex DESC
                        )
                        SELECT pwh.*
                        FROM PlayerWarHistories pwh
                        INNER JOIN ClanHistories ch ON pwh.ClanHistoryID = ch.ID
                        INNER JOIN PlayerWarWeeks pww ON ch.SeasonID = pww.SeasonID 
                            AND ch.WeekIndex = pww.WeekIndex
                        WHERE pwh.PlayerID = @playerId
                            AND ch.WarTrophies {trophyCondition}
                        ORDER BY ch.SeasonID DESC, ch.WeekIndex DESC, pwh.Fame DESC";

                var results = await _context.PlayerWarHistories
                    .FromSqlRaw(sql,
                        new SqlParameter("@numWeeks", numOfWeeksToUse),
                        new SqlParameter("@playerId", player.ID))
                    .ToListAsync();

                _logger.LogInformation("Found {Count} war histories for player {PlayerName}", results.Count, player.Name);
                return results.Any() ? results : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve war histories for player {player.Tag} from the database");
                throw new InvalidOperationException($"Failed to retrieve war histories for player tag {player.Tag} from the database", ex);
            }
        }
    }
}
