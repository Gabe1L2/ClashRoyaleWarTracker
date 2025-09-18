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
        private readonly ITimeZoneService _timeZoneService;
        public WarRepository(ApplicationDbContext context, ILogger<WarRepository> logger, ITimeZoneService timeZoneService)
        {
            _context = context;
            _logger = logger;
            _timeZoneService = timeZoneService;
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
                        playerWarHistory.LastUpdated = _timeZoneService.Now;
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
                            AND NOT (pwh.Fame = 0 AND pwh.DecksUsed = 0)
                        ORDER BY ch.SeasonID DESC, ch.WeekIndex DESC, pwh.Fame DESC";

                var results = await _context.PlayerWarHistories
                    .FromSqlRaw(sql,
                        new SqlParameter("@numWeeks", numOfWeeksToUse),
                        new SqlParameter("@playerId", player.ID))
                    .ToListAsync();

                _logger.LogDebug("Found {Count} war histories for player {PlayerName}", results.Count, player.Name);
                return results.Any() ? results : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve war histories for player {player.Tag} from the database");
                throw new InvalidOperationException($"Failed to retrieve war histories for player tag {player.Tag} from the database", ex);
            }
        }

        public async Task<List<PlayerWarHistoryExpanded>> GetAllPlayerWarHistoriesExpandedAsync(bool is5k)
        {
            try
            {
                _logger.LogDebug("Getting all Player War Histories (Expanded) from the database for {TrophyLevel} trophies", is5k ? "5k+" : "sub-5k");

                var trophyCondition = is5k ? ">= 5000" : "< 5000";

                var sql = $@"
            SELECT 
                pwh.ID,
                pwh.PlayerID,
                p.Tag as PlayerTag,
                p.Name as PlayerName,
                p.Status,
                pwh.ClanHistoryID,
                ch.SeasonID,
                ch.WeekIndex,
                ISNULL(c.ID, -1) as ClanID,
                ISNULL(c.Name, 'Unknown Clan') as ClanName,
                ch.WarTrophies,
                pwh.Fame,
                pwh.DecksUsed,
                pwh.BoatAttacks,
                pwh.UpdatedBy,
                pwh.LastUpdated
            FROM PlayerWarHistories pwh
            INNER JOIN ClanHistories ch ON pwh.ClanHistoryID = ch.ID
            INNER JOIN Players p ON pwh.PlayerID = p.ID
            LEFT JOIN Clans c ON p.ClanID = c.ID
            WHERE ch.WarTrophies {trophyCondition}
                AND NOT (pwh.Fame = 0 AND pwh.DecksUsed = 0 AND pwh.BoatAttacks = 0)
            ORDER BY p.Name, ch.SeasonID DESC, ch.WeekIndex DESC";

                var results = await _context.Database.SqlQueryRaw<PlayerWarHistoryExpanded>(sql).ToListAsync();

                _logger.LogDebug("Retrieved {Count} Player War Histories (Expanded) from the database for {TrophyLevel} trophies",
                    results.Count, is5k ? "5k+" : "sub-5k");

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve expanded player war histories for {TrophyLevel} trophies from the database",
                    is5k ? "5k+" : "sub-5k");
                throw new InvalidOperationException("Failed to retrieve expanded player war histories from the database", ex);
            }
        }

        public async Task<List<PlayerWarHistoryExpanded>> GetPlayerWarHistoriesByPlayerIdAsync(int playerId)
        {
            try
            {
                _logger.LogDebug("Retrieving war histories for PlayerID {PlayerId}", playerId);

                var warHistories = await _context.PlayerWarHistories
                    .Join(_context.ClanHistories, pwh => pwh.ClanHistoryID, ch => ch.ID, (pwh, ch) => new { pwh, ch })
                    .Join(_context.Players, combined => combined.pwh.PlayerID, p => p.ID, (combined, p) => new { combined.pwh, combined.ch, p })
                    .GroupJoin(_context.Clans, combined => combined.ch.ClanID, c => c.ID, (combined, clans) => new { combined.pwh, combined.ch, combined.p, clans })
                    .SelectMany(combined => combined.clans.DefaultIfEmpty(), (combined, c) => new PlayerWarHistoryExpanded
                    {
                        ID = combined.pwh.ID,
                        PlayerID = combined.p.ID,
                        PlayerTag = combined.p.Tag,
                        PlayerName = combined.p.Name ?? "Unknown",
                        Status = combined.p.Status,
                        ClanHistoryID = combined.ch.ID,
                        SeasonID = combined.ch.SeasonID,
                        WeekIndex = combined.ch.WeekIndex,
                        ClanID = combined.ch.ClanID,
                        ClanName = c != null ? c.Name : "Unknown Clan",
                        WarTrophies = c != null ? c.WarTrophies : 0,
                        Fame = combined.pwh.Fame,
                        DecksUsed = combined.pwh.DecksUsed,
                        BoatAttacks = combined.pwh.BoatAttacks,
                        UpdatedBy = combined.pwh.UpdatedBy,
                        LastUpdated = combined.pwh.LastUpdated
                    })
                    .Where(pwhe => pwhe.PlayerID == playerId)
                    .OrderByDescending(pwhe => pwhe.SeasonID)
                    .ThenByDescending(pwhe => pwhe.WeekIndex)
                    .ToListAsync();

                _logger.LogDebug("Found {Count} war history records for PlayerID {PlayerId}", warHistories.Count, playerId);
                return warHistories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve war histories for PlayerID {PlayerId}", playerId);
                throw new InvalidOperationException($"Failed to retrieve war histories for PlayerID {playerId}", ex);
            }
        }

        public async Task<bool> UpdatePlayerWarHistoryAsync(int warHistoryId, int fame, int decksUsed, int boatAttacks)
        {
            try
            {
                _logger.LogDebug("Updating war history ID {WarHistoryId}", warHistoryId);

                var warHistory = await _context.PlayerWarHistories.FirstOrDefaultAsync(pwh => pwh.ID == warHistoryId);
                if (warHistory == null)
                {
                    _logger.LogWarning("War history with ID {WarHistoryId} not found", warHistoryId);
                    return false;
                }

                warHistory.Fame = fame;
                warHistory.DecksUsed = decksUsed;
                warHistory.BoatAttacks = boatAttacks;
                warHistory.LastUpdated = _timeZoneService.Now;
                warHistory.IsModified = true;
                warHistory.UpdatedBy = "Admin"; // Ideally, this should be set to the current user's name

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated war history ID {WarHistoryId}", warHistoryId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update war history ID {WarHistoryId}", warHistoryId);
                throw new InvalidOperationException($"Failed to update war history ID {warHistoryId}", ex);
            }
        }
    }
}