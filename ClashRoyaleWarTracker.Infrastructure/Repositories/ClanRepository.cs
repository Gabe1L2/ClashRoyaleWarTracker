using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class ClanRepository : IClanRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClanRepository> _logger;

        public ClanRepository(ApplicationDbContext context, ILogger<ClanRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddClanAsync(Clan clan)
        {
            try
            {
                _logger.LogDebug($"Attempting to add clan with tag {clan.Tag}");
                var curClan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clan.Tag);

                if (curClan == null)
                {
                    clan.LastUpdated = DateTime.Now;
                    await _context.Clans.AddAsync(clan);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Successfully added clan {clan.Name}");
                    return true;
                }

                _logger.LogWarning($"Clan with tag {clan.Tag} already exists. Skipping add.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add clan {clan.Tag}");
                throw new InvalidOperationException($"Failed to add clan {clan.Tag}", ex);
            }
        }

        public async Task<Clan?> GetClanAsync(string clanTag)
        {
            try
            {
                _logger.LogDebug("Retrieving clan with tag {ClanTag}", clanTag);

                var clan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clanTag);

                if (clan != null)
                {
                    _logger.LogDebug("Found clan {ClanName} with tag {ClanTag}", clan.Name, clanTag);
                }
                else
                {
                    _logger.LogDebug("No clan found with tag {ClanTag}", clanTag);
                }

                return clan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve clan with tag {clanTag} from the database");
                throw new InvalidOperationException($"Failed to retrieve clan with tag {clanTag} from the database", ex);
            }
        }

        public async Task<IEnumerable<Clan>> GetAllClansAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all clans from the database");
                var clans = await _context.Clans.ToListAsync();

                _logger.LogInformation($"Retrieved {clans.Count} clans from database");
                return clans;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all clans from database");
                throw new InvalidOperationException("Failed to retrieve clans from the database", ex);
            }
        }

        public async Task<bool> DeleteClanAsync(string clanTag)
        {
            try
            {
                _logger.LogDebug($"Attempting to delete clan with tag {clanTag}");
                var delCount = await _context.Clans
                    .Where(c => c.Tag == clanTag)
                    .ExecuteDeleteAsync();

                if (delCount > 0)
                {
                    _logger.LogInformation($"Successfully deleted clan with tag {clanTag}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"No clan found with tag {clanTag} to delete");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete clan with tag {clanTag}");
                throw new InvalidOperationException($"Failed to delete clan with tag {clanTag}", ex);
            }

        }

        public async Task<bool> UpdateClanAsync(Clan clan)
        {
            try
            {
                _logger.LogDebug($"Attempting to update clan {clan.Name}");
                var curClan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clan.Tag);
                if (curClan == null)
                {
                    _logger.LogWarning($"Clan with tag {clan.Tag} does not exist. Cannot update.");
                    return false;
                }

                curClan.Name = clan.Name;
                curClan.WarTrophies = clan.WarTrophies;
                curClan.LastUpdated = DateTime.Now;

                _context.Clans.Update(curClan);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully updated clan {clan.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update clan {clan.Tag}");
                throw new InvalidOperationException($"Failed to add/update clan {clan.Tag}", ex);
            }
        }

        public async Task<bool> PopulateClanHistoryAsync(Clan clan, List<ClanHistory> clanHistories)
        {
            try
            {
                _logger.LogDebug($"Populating clan history for {clan.Name}");
                var curClan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clan.Tag);
                if (curClan == null)
                {
                    _logger.LogWarning($"Clan with tag {clan.Tag} does not exist. Cannot populate history.");
                    return false;
                }

                foreach (var clanHistory in clanHistories)
                {
                    var existingHistory = await _context.ClanHistories.FirstOrDefaultAsync(ch =>
                        ch.ClanID == curClan.ID &&
                        ch.SeasonID == clanHistory.SeasonID &&
                        ch.WeekIndex == clanHistory.WeekIndex);

                    if (existingHistory == null)
                    {
                        clanHistory.ClanID = curClan.ID;
                        await _context.ClanHistories.AddAsync(clanHistory);
                        _logger.LogDebug($"Added new clan history for {clan.Name} - Season {clanHistory.SeasonID}, Week {clanHistory.WeekIndex}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully populated clan history for {clan.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to populate clan history for {clan.Tag}");
                throw new InvalidOperationException($"Failed to update clan history for {clan.Tag}", ex);
            }
        }

        public async Task<ClanHistory?> GetClanHistoryAsync(int clanID, int seasonID, int weekIndex)
        {
            try
            {
                _logger.LogDebug($"Retrieving clan history from database for ClanID {clanID}, SeasonID {seasonID}, WeekIndex {weekIndex}");
                var clanHistory = await _context.ClanHistories.FirstOrDefaultAsync(ch =>
                    ch.ClanID == clanID &&
                    ch.SeasonID == seasonID &&
                    ch.WeekIndex == weekIndex);

                if (clanHistory != null)
                {
                    _logger.LogInformation($"Found clan history for ClanID {clanID}, SeasonID {seasonID}, WeekIndex {weekIndex}");
                    return clanHistory;
                }
                else
                {
                    _logger.LogWarning($"No clan history found for ClanID {clanID}, SeasonID {seasonID}, WeekIndex {weekIndex}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve clan history for ClanID {clanID}, SeasonID {seasonID}, WeekIndex {weekIndex}");
                throw new InvalidOperationException($"Failed to retrieve clan history for ClanID {clanID}, SeasonID {seasonID}, WeekIndex {weekIndex}", ex);
            }
        }

        public async Task<int> GetMostRecentClanIDAsync(Player player, bool aboveFiveThousandTrophies)
        {
            try
            {
                _logger.LogDebug($"Getting most recent clan ID for player {player.Name} - {player.Tag} {(aboveFiveThousandTrophies ? "above/equal to" : "below")} 5000 war trophies");

                var trophyCondition = aboveFiveThousandTrophies ? ">= 5000" : "< 5000";

                var sql = $@"
                    SELECT TOP 1 ch.ClanID AS Value
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
                    ORDER BY ch.SeasonID DESC, ch.WeekIndex DESC, ch.WarTrophies DESC";

                var result = await _context.Database
                    .SqlQueryRaw<int>(sql, new SqlParameter("@playerId", player.ID))
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Found most recent clan ID {ClanId} for player {PlayerName}", result, player.Name);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve most recent ClanID for player {player.Tag} from the database");
                throw new InvalidOperationException($"Failed to retrieve most recent ClanID for player tag {player.Tag} from the database", ex);
            }
        }
    }
}
