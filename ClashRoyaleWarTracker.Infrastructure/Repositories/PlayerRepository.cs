using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlayerRepository> _logger;
        private readonly ITimeZoneService _timeZoneService;
        public PlayerRepository(ApplicationDbContext context, ILogger<PlayerRepository> logger, ITimeZoneService timeZoneService)
        {
            _context = context;
            _logger = logger;
            _timeZoneService = timeZoneService;
        }

        public async Task<Player?> GetPlayerAsync(string playerTag)
        {
            try
            {
                _logger.LogDebug("Retrieving player with tag {PlayerTag} from database", playerTag);
                var player = await _context.Players.FirstOrDefaultAsync(p => p.Tag == playerTag);
                if (player != null)
                {
                    _logger.LogDebug("Found player {PlayerName} with tag {PlayerTag}", player.Name, playerTag);
                }
                else
                {
                    _logger.LogDebug("No player found with tag {PlayerTag}", playerTag);
                }
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve player with tag {PlayerTag} from the database", playerTag);
                throw new InvalidOperationException($"Failed to retrieve player with tag {playerTag} from the database", ex);
            }
        }

        public async Task<int> AddPlayerAsync(Player player)
        {
            try
            {
                _logger.LogDebug("Adding new player {PlayerName} with tag {PlayerTag} to database", player.Name, player.Tag);
                player.LastUpdated = _timeZoneService.Now;
                await _context.Players.AddAsync(player);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully added player {PlayerName} with tag {PlayerTag}", player.Name, player.Tag);
                return player.ID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add player {PlayerTag} to the database", player.Tag);
                throw new InvalidOperationException($"Failed to add player {player.Tag}", ex);
            }
        }

        public async Task<List<Player>> GetAllActivePlayersAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all active players from database");

                var players = await _context.Players
                    .Where(p => p.Status == "Active")
                    .ToListAsync();

                _logger.LogDebug("Found {Count} active players", players.Count);
                return players;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active players from the database");
                throw new InvalidOperationException("Failed to retrieve active players from the database", ex);
            }
        }

        public async Task<List<Player>> GetL2WPlayersAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all L2W (Left 2 Weeks) players from database");

                var players = await _context.Players
                    .Where(p => p.Status == "L2W")
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                _logger.LogDebug("Found {Count} L2W players", players.Count);
                return players;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve L2W players from the database");
                throw new InvalidOperationException("Failed to retrieve L2W players from the database", ex);
            }
        }

        public async Task UpsertPlayerAverageAsync(PlayerAverage newPlayerAverage)
        {
            try
            {
                _logger.LogDebug("Upserting player average for PlayerID {PlayerId}, Is5k: {Is5k}", newPlayerAverage.PlayerID, newPlayerAverage.Is5k);

                var existingAverage = await _context.PlayerAverages
                    .FirstOrDefaultAsync(pa => pa.PlayerID == newPlayerAverage.PlayerID && pa.Is5k == newPlayerAverage.Is5k);

                if (existingAverage != null)
                {
                    // Update existing record
                    existingAverage.ClanID = newPlayerAverage.ClanID;
                    existingAverage.FameAttackAverage = newPlayerAverage.FameAttackAverage;
                    existingAverage.Attacks = newPlayerAverage.Attacks;
                    existingAverage.LastUpdated = _timeZoneService.Now;
                    _context.PlayerAverages.Update(existingAverage);
                    _logger.LogDebug("Updated existing player average for PlayerID {PlayerId}", newPlayerAverage.PlayerID);
                }
                else
                {
                    newPlayerAverage.LastUpdated = _timeZoneService.Now;
                    await _context.PlayerAverages.AddAsync(newPlayerAverage);
                    _logger.LogDebug("Added new player average for PlayerID {PlayerId}", newPlayerAverage.PlayerID);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert player average for PlayerID {PlayerId}", newPlayerAverage.PlayerID);
                throw new InvalidOperationException($"Failed to upsert player average for PlayerID {newPlayerAverage.PlayerID}", ex);
            }
        }

        public async Task<List<PlayerAverageDTO>> GetAllPlayerAverageDTOsAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all player averages with player and clan details");

                var playerAverages = await _context.PlayerAverages
                    .Join(_context.Players, pa => pa.PlayerID, p => p.ID, (pa, p) => new { pa, p })
                    .GroupJoin(_context.Clans, x => x.pa.ClanID, c => c.ID, (x, clans) => new { x.pa, x.p, clans })
                    .SelectMany(x => x.clans.DefaultIfEmpty(), (x, c) => new PlayerAverageDTO
                    {
                        ID = x.pa.ID,
                        PlayerID = x.pa.PlayerID,
                        PlayerName = x.p.Name ?? "Unknown",
                        PlayerTag = x.p.Tag,
                        ClanID = x.pa.ClanID,
                        ClanName = c != null ? c.Name : "No Clan",
                        FameAttackAverage = x.pa.FameAttackAverage,
                        Attacks = x.pa.Attacks,
                        Is5k = x.pa.Is5k,
                        LastUpdated = x.pa.LastUpdated,
                        Status = x.p.Status
                    })
                    .OrderByDescending(dto => dto.FameAttackAverage)
                    .ToListAsync();

                _logger.LogDebug("Found {Count} player averages", playerAverages.Count);
                return playerAverages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve player averages from the database");
                throw new InvalidOperationException("Failed to retrieve player averages from the database", ex);
            }
        }

        public async Task<Player?> GetPlayerByIdAsync(int playerId)
        {
            try
            {
                _logger.LogDebug("Retrieving player with ID {PlayerId} from database", playerId);
                var player = await _context.Players.FirstOrDefaultAsync(p => p.ID == playerId);
                if (player != null)
                {
                    _logger.LogDebug("Found player {PlayerName} with ID {PlayerId}", player.Name, playerId);
                }
                else
                {
                    _logger.LogDebug("No player found with ID {PlayerId}", playerId);
                }
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve player with ID {PlayerId} from the database", playerId);
                throw new InvalidOperationException($"Failed to retrieve player with ID {playerId} from the database", ex);
            }
        }

        public async Task<bool> UpdatePlayerStatusAsync(int playerId, string status)
        {
            try
            {
                _logger.LogDebug("Updating player status for PlayerID {PlayerId} to {Status}", playerId, status);

                var player = await _context.Players.FirstOrDefaultAsync(p => p.ID == playerId);
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found", playerId);
                    return false;
                }

                player.Status = status;
                player.LastUpdated = _timeZoneService.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated player {PlayerName} status to {Status}", player.Name, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update player status for PlayerID {PlayerId}", playerId);
                throw new InvalidOperationException($"Failed to update player status for PlayerID {playerId}", ex);
            }
        }

        public async Task<bool> UpdatePlayerNotesAsync(int playerId, string? notes)
        {
            try
            {
                _logger.LogDebug("Updating notes for PlayerID {PlayerId}", playerId);

                var player = await _context.Players.FirstOrDefaultAsync(p => p.ID == playerId);
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found", playerId);
                    return false;
                }

                var trimmed = notes?.Trim();
                if (!string.IsNullOrEmpty(trimmed) && trimmed.Length > 100)
                {
                    trimmed = trimmed[..100];
                }

                player.Notes = trimmed;
                player.LastUpdated = _timeZoneService.Now;

                _context.Players.Update(player);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated notes for player {PlayerName} ({PlayerId})", player.Name, playerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update notes for PlayerID {PlayerId}", playerId);
                throw new InvalidOperationException($"Failed to update notes for player {playerId}", ex);
            }
        }

        public async Task<IEnumerable<RosterAssignmentDTO>> GetAllRosterAssignmentDTOsAsync()
        {
            try
            {
                _logger.LogDebug("Getting all Roster Assignments");

                var sql = @"
                            SELECT
                                r.ID,
                                r.SeasonID,
                                r.WeekIndex,
                                r.PlayerID,
                                p.Tag AS PlayerTag,
                                ISNULL(p.Name, '') AS PlayerName,
                                p.Status,
                                p.Notes,
                                CAST(ISNULL(pa.FameAttackAverage, 0.00) AS decimal(5,2)) AS FameAttackAverage,
                                CAST(ISNULL(pa.Is5k, 0) AS bit) AS Is5k,
                                r.ClanID,
                                c.Name AS ClanName,
                                c.Tag  AS ClanTag,
                                r.IsInClan,
                                r.LastUpdated,
                                r.UpdatedBy
                            FROM RosterAssignments r
                            INNER JOIN Players p ON r.PlayerID = p.ID
                            LEFT JOIN Clans c ON r.ClanID = c.ID
                            OUTER APPLY (
                                -- if roster.ClanID IS NULL => treat as 5k bracket (Is5k = 1)
                                -- otherwise choose bracket based on the clan's WarTrophies
                                SELECT TOP 1 pa2.FameAttackAverage, pa2.Is5k
                                FROM PlayerAverages pa2
                                WHERE pa2.PlayerID = p.ID
                                  AND pa2.Is5k = CASE 
                                                    WHEN r.ClanID IS NULL THEN CAST(1 AS bit)
                                                    WHEN ISNULL(c.WarTrophies, 0) >= 5000 THEN CAST(1 AS bit)
                                                    ELSE CAST(0 AS bit)
                                                 END
                                ORDER BY pa2.LastUpdated DESC
                            ) pa
                            ORDER BY r.LastUpdated DESC;";

                var result = await _context.Database.SqlQueryRaw<RosterAssignmentDTO>(sql).ToListAsync();

                _logger.LogDebug("Retrieved {Count} roster assignment DTOs", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve roster assignments via raw SQL");
                throw;
            }
        }

        public async Task<IEnumerable<PlayerAverage>> GetAllActivePlayerAveragesAsync(bool is5k)
        {
            try
            {
                _logger.LogDebug("Retrieving all active player averages for {TrophyLevel}", is5k ? "5k+" : "sub-5k");

                var playerAverages = await _context.PlayerAverages
                    .Join(_context.Players, pa => pa.PlayerID, p => p.ID, (pa, p) => new { pa, p })
                    .Where(x => x.p.Status == "Active" && x.pa.Is5k == is5k)
                    .Select(x => x.pa)
                    .ToListAsync();

                _logger.LogDebug("Found {Count} active player averages for {TrophyLevel}", playerAverages.Count, is5k ? "5k+" : "sub-5k");
                return playerAverages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active player averages for {TrophyLevel}", is5k ? "5k+" : "sub-5k");
                throw;
            }
        }

        public async Task<List<Player>> GetAllPlayersAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all players from database");
                var players = await _context.Players
                    .OrderBy(p => p.Name)
                    .ThenBy(p => p.Tag)
                    .ToListAsync();
                _logger.LogDebug("Found {Count} players", players.Count);
                return players;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve players from the database");
                throw new InvalidOperationException("Failed to retrieve players from the database", ex);
            }
        }

        public async Task<bool> BulkUpsertRosterAssignmentsAsync(List<RosterAssignment> rosterAssignments)
        {
            try
            {
                _logger.LogDebug("Bulk upserting {Count} roster assignments", rosterAssignments.Count);

                // First, delete existing assignments for season 999, week 999
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RosterAssignments WHERE SeasonID = 999 AND WeekIndex = 999");

                // Add new assignments
                foreach (var assignment in rosterAssignments)
                {
                    assignment.LastUpdated = _timeZoneService.Now;
                    await _context.RosterAssignments.AddAsync(assignment);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully bulk upserted {Count} roster assignments", rosterAssignments.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk upsert roster assignments");
                throw new InvalidOperationException("Failed to bulk upsert roster assignments", ex);
            }
        }

        public async Task<bool> UpdateRosterAssignmentInClanStatusAsync(int rosterAssignmentId, bool isInClan)
        {
            try
            {
                _logger.LogDebug("Updating IsInClan status for roster assignment {RosterAssignmentId} to {IsInClan}",
                    rosterAssignmentId, isInClan);

                var rosterAssignment = await _context.RosterAssignments
                    .FirstOrDefaultAsync(r => r.ID == rosterAssignmentId);

                if (rosterAssignment == null)
                {
                    _logger.LogWarning("Roster assignment with ID {RosterAssignmentId} not found", rosterAssignmentId);
                    return false;
                }

                rosterAssignment.IsInClan = isInClan;
                rosterAssignment.LastUpdated = _timeZoneService.Now;

                await _context.SaveChangesAsync();

                _logger.LogDebug("Successfully updated IsInClan status for roster assignment {RosterAssignmentId}",
                    rosterAssignmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update IsInClan status for roster assignment {RosterAssignmentId}",
                    rosterAssignmentId);
                return false;
            }
        }

        public async Task<List<RosterAssignmentDTO>> GetRosterAssignmentsForOneWeekOneClanAsync(int seasonId, int weekIndex, int? clanId)
        {
            try
            {
                string filterDescription = clanId.HasValue ? $"ClanID {clanId}" : "unassigned players";

                _logger.LogDebug("Getting roster assignments for Season {SeasonId}, Week {WeekIndex}, Filter: {FilterDescription}",
                    seasonId, weekIndex, filterDescription);

                var sql = @"
                SELECT
                    r.ID,
                    r.SeasonID,
                    r.WeekIndex,
                    r.PlayerID,
                    p.Tag AS PlayerTag,
                    ISNULL(p.Name, '') AS PlayerName,
                    p.Status,
                    p.Notes,
                    CAST(ISNULL(pa.FameAttackAverage, 0.00) AS decimal(5,2)) AS FameAttackAverage,
                    CAST(ISNULL(pa.Is5k, 0) AS bit) AS Is5k,
                    r.ClanID,
                    c.Name AS ClanName,
                    c.Tag AS ClanTag,
                    r.IsInClan,
                    r.LastUpdated,
                    r.UpdatedBy
                FROM RosterAssignments r
                INNER JOIN Players p ON r.PlayerID = p.ID
                LEFT JOIN Clans c ON r.ClanID = c.ID
                OUTER APPLY (
                    SELECT TOP 1 pa2.FameAttackAverage, pa2.Is5k
                    FROM PlayerAverages pa2
                    WHERE pa2.PlayerID = p.ID
                      AND pa2.Is5k = CASE 
                                        WHEN r.ClanID IS NULL THEN CAST(1 AS bit)
                                        WHEN ISNULL(c.WarTrophies, 0) >= 5000 THEN CAST(1 AS bit)
                                        ELSE CAST(0 AS bit)
                                     END
                    ORDER BY pa2.LastUpdated DESC
                ) pa
                WHERE r.SeasonID = @p0 AND r.WeekIndex = @p1 
                  AND (@p2 IS NULL AND r.ClanID IS NULL OR r.ClanID = @p2)
                ORDER BY r.ID;";

                var result = await _context.Database.SqlQueryRaw<RosterAssignmentDTO>(sql, seasonId, weekIndex, clanId).ToListAsync();

                _logger.LogDebug("Retrieved {Count} roster assignments for Season {SeasonId}, Week {WeekIndex}, Filter: {FilterDescription}",
                    result.Count, seasonId, weekIndex, filterDescription);
                return result;
            }
            catch (Exception ex)
            {
                string filterDescription = clanId.HasValue ? $"ClanID {clanId}" : "unassigned players";

                _logger.LogError(ex, "Failed to retrieve roster assignments for Season {SeasonId}, Week {WeekIndex}, Filter: {FilterDescription}",
                    seasonId, weekIndex, filterDescription);
                throw;
            }
        }

        public async Task<bool> CopyRosterAssignmentsToNewSeasonWeekAsync(int currentSeasonId, int currentWeekIndex, int newSeasonId, int newWeekIndex)
        {
            try
            {
                _logger.LogInformation("Copying roster assignments from Season {CurrentSeasonId}, Week {CurrentWeekIndex} to Season {NewSeasonId}, Week {NewWeekIndex}",
                    currentSeasonId, currentWeekIndex, newSeasonId, newWeekIndex);

                // Get all current roster assignments (999, 999)
                var currentAssignments = await _context.RosterAssignments
                    .Where(r => r.SeasonID == currentSeasonId && r.WeekIndex == currentWeekIndex)
                    .ToListAsync();

                if (!currentAssignments.Any())
                {
                    _logger.LogWarning("No roster assignments found for Season {CurrentSeasonId}, Week {CurrentWeekIndex}",
                        currentSeasonId, currentWeekIndex);
                    return true; // Not an error, just nothing to copy
                }

                // Create new roster assignments with the new season/week
                var newAssignments = currentAssignments.Select(r => new RosterAssignment
                {
                    SeasonID = newSeasonId,
                    WeekIndex = newWeekIndex,
                    PlayerID = r.PlayerID,
                    ClanID = r.ClanID,
                    IsInClan = r.IsInClan,
                    LastUpdated = _timeZoneService.Now,
                    UpdatedBy = "WeeklyUpdate"
                }).ToList();

                // Add new assignments to database
                await _context.RosterAssignments.AddRangeAsync(newAssignments);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully copied {Count} roster assignments from Season {CurrentSeasonId}, Week {CurrentWeekIndex} to Season {NewSeasonId}, Week {NewWeekIndex}",
                    newAssignments.Count, currentSeasonId, currentWeekIndex, newSeasonId, newWeekIndex);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy roster assignments from Season {CurrentSeasonId}, Week {CurrentWeekIndex} to Season {NewSeasonId}, Week {NewWeekIndex}",
                    currentSeasonId, currentWeekIndex, newSeasonId, newWeekIndex);
                return false;
            }
        }

        public async Task<IEnumerable<(int SeasonId, int WeekIndex)>> GetDistinctRosterSeasonWeeksAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving distinct roster season/weeks from database");

                var seasonWeeks = await _context.RosterAssignments
                    .Select(r => new { r.SeasonID, r.WeekIndex })
                    .Distinct()
                    .OrderByDescending(sw => sw.SeasonID == 999 && sw.WeekIndex == 999 ? 1 : 0) // Current roster first
                    .ThenByDescending(sw => sw.SeasonID)
                    .ThenByDescending(sw => sw.WeekIndex)
                    .ToListAsync();

                var result = seasonWeeks.Select(sw => (sw.SeasonID, sw.WeekIndex)).ToList();

                _logger.LogDebug("Found {Count} distinct season/week combinations", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve distinct roster season/weeks");
                throw new InvalidOperationException("Failed to retrieve distinct roster season/weeks", ex);
            }
        }

        public async Task<IEnumerable<RosterAssignmentDTO>> GetRosterAssignmentsBySeasonWeekAsync(int seasonId, int weekIndex)
        {
            try
            {
                _logger.LogDebug("Getting roster assignments for Season {SeasonId}, Week {WeekIndex}", seasonId, weekIndex);

                var sql = @"
                            SELECT
                                r.ID,
                                r.SeasonID,
                                r.WeekIndex,
                                r.PlayerID,
                                p.Tag AS PlayerTag,
                                ISNULL(p.Name, '') AS PlayerName,
                                p.Status,
                                p.Notes,
                                CAST(ISNULL(pa.FameAttackAverage, 0.00) AS decimal(5,2)) AS FameAttackAverage,
                                CAST(ISNULL(pa.Is5k, 0) AS bit) AS Is5k,
                                r.ClanID,
                                c.Name AS ClanName,
                                c.Tag  AS ClanTag,
                                r.IsInClan,
                                r.LastUpdated,
                                r.UpdatedBy
                            FROM RosterAssignments r
                            INNER JOIN Players p ON r.PlayerID = p.ID
                            LEFT JOIN Clans c ON r.ClanID = c.ID
                            OUTER APPLY (
                                SELECT TOP 1 pa2.FameAttackAverage, pa2.Is5k
                                FROM PlayerAverages pa2
                                WHERE pa2.PlayerID = p.ID
                                  AND pa2.Is5k = CASE 
                                                    WHEN r.ClanID IS NULL THEN CAST(1 AS bit)
                                                    WHEN ISNULL(c.WarTrophies, 0) >= 5000 THEN CAST(1 AS bit)
                                                    ELSE CAST(0 AS bit)
                                                 END
                                ORDER BY pa2.LastUpdated DESC
                            ) pa
                            WHERE r.SeasonID = @p0 AND r.WeekIndex = @p1
                            ORDER BY r.LastUpdated DESC;";

                var result = await _context.Database.SqlQueryRaw<RosterAssignmentDTO>(sql, seasonId, weekIndex).ToListAsync();

                _logger.LogDebug("Retrieved {Count} roster assignments for Season {SeasonId}, Week {WeekIndex}",
                    result.Count, seasonId, weekIndex);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve roster assignments for Season {SeasonId}, Week {WeekIndex}",
                    seasonId, weekIndex);
                throw;
            }
        }

        public async Task<bool> UpdateRosterAssignmentClanAsync(int rosterAssignmentId, int? clanId)
        {
            try
            {
                _logger.LogDebug("Updating roster assignment {RosterAssignmentId} to ClanID {ClanId}",
                    rosterAssignmentId, clanId);

                var rosterAssignment = await _context.RosterAssignments
                    .FirstOrDefaultAsync(r => r.ID == rosterAssignmentId);

                if (rosterAssignment == null)
                {
                    _logger.LogWarning("Roster assignment with ID {RosterAssignmentId} not found", rosterAssignmentId);
                    return false;
                }

                rosterAssignment.ClanID = clanId;
                rosterAssignment.LastUpdated = _timeZoneService.Now;
                rosterAssignment.UpdatedBy = "ManualUpdate";

                _context.RosterAssignments.Update(rosterAssignment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated roster assignment {RosterAssignmentId} to ClanID {ClanId}",
                    rosterAssignmentId, clanId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update roster assignment {RosterAssignmentId} to ClanID {ClanId}",
                    rosterAssignmentId, clanId);
                return false;
            }
        }
    }
}