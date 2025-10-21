using ClashRoyaleWarTracker.Application.Helpers;
using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Text.RegularExpressions;

namespace ClashRoyaleWarTracker.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IClashRoyaleService _clashRoyaleService;
        private readonly IClanRepository _clanRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IWarRepository _warRepository;
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(IClashRoyaleService clashRoyaleService, IClanRepository clanRepository, IPlayerRepository playerRepository, IWarRepository warRepository, ILogger<ApplicationService> logger)
        {
            _clashRoyaleService = clashRoyaleService;
            _clanRepository = clanRepository;
            _playerRepository = playerRepository;
            _warRepository = warRepository;
            _logger = logger;
        }

        public async Task<ServiceResult> WeeklyUpdateAsync(int numWeeksForPlayerAverages = 4)
        {
            return await DataUpdateAsync(1, numWeeksForPlayerAverages);
        }
        
        public async Task<ServiceResult> DataUpdateAsync(int numWeeksWarHistory, int numWeeksPlayerAverages = 4)
        {
            try
            {
                _logger.LogInformation("Starting data update for all clans");

                var getAllClansResult = await GetAllClansAsync();
                if (!getAllClansResult.Success || getAllClansResult.Data == null || !getAllClansResult.Data.Any())
                {
                    _logger.LogWarning("Failed to retrieve clans for data update");
                    return ServiceResult.Failure("Failed to retrieve clans for data update");
                }

                var clans = getAllClansResult.Data.ToList();
                var totalClans = clans.Count;
                var successfulUpdates = 0;
                var failedUpdates = 0;
                var successfulHistoryUpdates = 0;
                var failedHistoryUpdates = 0;
                var successfulWarHistoryUpdates = 0;
                var failedWarHistoryUpdates = 0;

                _logger.LogInformation("Found {TotalClans} clans to update", totalClans);

                foreach (var clan in clans)
                {
                    _logger.LogInformation("Processing clan {ClanName} ({ClanTag})", clan.Name, clan.Tag);

                    // Update clan basic information
                    var updateResult = await UpdateClanAsync(clan.Tag);
                    if (updateResult.Success)
                    {
                        successfulUpdates++;
                        _logger.LogInformation("Successfully updated clan {ClanName}", clan.Name);
                    }
                    else
                    {
                        failedUpdates++;
                        _logger.LogWarning("Failed to update clan {ClanName}: {ErrorMessage}", clan.Name, updateResult.Message);
                    }

                    // Update clan history regardless of basic update result
                    var historyResult = await PopulateClanHistoryAsync(clan);
                    if (historyResult.Success)
                    {
                        successfulHistoryUpdates++;
                        _logger.LogInformation("Successfully updated history for clan {ClanName}", clan.Name);
                    }
                    else
                    {
                        failedHistoryUpdates++;
                        _logger.LogWarning("Failed to update history for clan {ClanName}: {ErrorMessage}", clan.Name, historyResult.Message);
                    }

                    // Populate new player war histories
                    var warHistoryResult = await PopulatePlayerWarHistories(clan, numWeeksWarHistory);
                    if (warHistoryResult.Success)
                    {
                        successfulWarHistoryUpdates++;
                        _logger.LogInformation("Successfully populated player war histories for clan {ClanName}", clan.Name);
                    }
                    else
                    {
                        failedWarHistoryUpdates++;
                        _logger.LogWarning("Failed to populate player war histories for clan {ClanName}: {ErrorMessage}", clan.Name, warHistoryResult.Message);
                    }
                }

                // Get the most recent season/week from the database (after clan history is populated)
                int? mostRecentSeasonId = null;
                int? mostRecentWeekIndex = null;
                
                foreach (var clan in clans)
                {
                    var clanHistories = await _clanRepository.GetAllClanHistoriesForClanAsync(clan.ID);
                    if (clanHistories != null && clanHistories.Any())
                    {
                        var mostRecent = clanHistories
                            .OrderByDescending(ch => ch.SeasonID)
                            .ThenByDescending(ch => ch.WeekIndex)
                            .First();
                        
                        mostRecentSeasonId = mostRecent.SeasonID;
                        mostRecentWeekIndex = mostRecent.WeekIndex;
                        _logger.LogInformation("Found most recent war data from database: Season {SeasonId}, Week {WeekIndex} from clan {ClanName}",
                            mostRecentSeasonId, mostRecentWeekIndex, clan.Name);
                        break; // We only need one clan's most recent data
                    }
                }

                // Backup current roster (999, 999) to the most recent season/week if we found it
                if (mostRecentSeasonId.HasValue && mostRecentWeekIndex.HasValue)
                {
                    var backupResult = await BackupCurrentRosterToNewSeasonWeekAsync(mostRecentSeasonId.Value, mostRecentWeekIndex.Value);
                    if (backupResult.Success)
                    {
                        _logger.LogInformation("Successfully backed up current roster to Season {SeasonId}, Week {WeekIndex}",
                            mostRecentSeasonId, mostRecentWeekIndex);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to backup current roster: {Message}", backupResult.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not determine most recent season/week for roster backup - no clan history data available");
                }

                // Update all active player averages for 5k+
                var update5kAveragesResult = await UpdateAllPlayerAveragesAsync(numWeeksPlayerAverages, true);
                if (update5kAveragesResult.Success)
                {
                    _logger.LogInformation("Successfully updated 5k+ player averages");
                }
                else
                {
                    _logger.LogWarning("Failed to update 5k+ player averages: {ErrorMessage}", update5kAveragesResult.Message);
                }

                // Update all active player averages for sub-5k
                var updateSub5kAveragesResult = await UpdateAllPlayerAveragesAsync(numWeeksPlayerAverages, false);
                if (updateSub5kAveragesResult.Success)
                {
                    _logger.LogInformation("Successfully updated sub-5k player averages");
                }
                else
                {
                    _logger.LogWarning("Failed to update sub-5k player averages: {ErrorMessage}", updateSub5kAveragesResult.Message);
                }

                string summary = $"Data update completed. " + 
                              $"Total Clans: {totalClans}, " +
                              $"Successful Clan Updates: {successfulUpdates}, Failed Clan Updates: {failedUpdates}, " +
                              $"Successful ClanHistory Updates: {successfulHistoryUpdates}, Failed ClanHistory Updates: {failedHistoryUpdates}," +
                              $"Successful PlayerWarHistory Updates: {successfulWarHistoryUpdates}, Failed PlayerWarHistory Updates: {failedWarHistoryUpdates}";

                _logger.LogInformation("Data update completed. Total Clans: {TotalClans}, Successful Clan Updates: {SuccessfulUpdates}, Failed Clan Updates: {FailedUpdates}, Successful ClanHistory Updates: {SuccessfulHistoryUpdates}, Failed ClanHistory Updates: {FailedHistoryUpdates}, Successful PlayerWarHistory Updates: {SuccessfulWarHistoryUpdates}, Failed PlayerWarHistory Updates: {FailedWarHistoryUpdates}", 
                    totalClans, successfulUpdates, failedUpdates, successfulHistoryUpdates, failedHistoryUpdates, successfulWarHistoryUpdates, failedWarHistoryUpdates);

                if (failedUpdates == 0 && failedHistoryUpdates == 0 && failedWarHistoryUpdates == 0)
                {
                    return ServiceResult.Successful(summary);
                }
                else if (successfulUpdates > 0 || successfulHistoryUpdates > 0 || successfulWarHistoryUpdates > 0)
                {
                    return ServiceResult.Successful($"Partial success: {summary}");
                }
                else
                {
                    return ServiceResult.Failure($"All updates failed: {summary}");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during the data update");
                return ServiceResult.Failure("An unexpected error occurred during the data update");
            }
        }

        public async Task<ServiceResult> AddClanAsync(string clanTag)
        {
            try
            {
                // Input Validation
                var sanitizedTag = ClanTagValidator.ValidateAndSanitizeClanTag(clanTag);
                if (!sanitizedTag.isValid)
                {
                    _logger.LogWarning("Invalid clan tag provided: {ClanTag}", clanTag);
                    return ServiceResult.Failure(sanitizedTag.errorMessage);
                }

                var tag = sanitizedTag.sanitizedTag;
                _logger.LogInformation("Adding clan with tag {ClanTag}", tag);
                var clan = await _clashRoyaleService.GetClanByTagAsync(tag);
                if (clan == null)
                {
                    _logger.LogWarning("Clan with tag {ClanTag} not found in API", tag);
                    return ServiceResult.Failure($"Clan with tag '{tag}' not found in API");
                }

                clan.Tag = Regex.Replace(clan.Tag, @"[^a-zA-Z0-9]", "");

                _logger.LogInformation("Clan {ClanName} with tag {ClanTag} found. Adding to database", clan.Name, clan.Tag);
                if (await _clanRepository.AddClanAsync(clan))
                {
                    _logger.LogInformation("Successfully added {ClanName} with tag {ClanTag} to database", clan.Name, clan.Tag);
                    return ServiceResult.Successful($"{clan.Name} successfully added to Clans!");
                }
                else
                {
                    _logger.LogWarning("Clan with tag {ClanTag} already exists in database", clan.Tag);
                    return ServiceResult.Failure($"Clan with tag '{clan.Tag}' already exists in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding clan with tag {ClanTag}", clanTag);
                return ServiceResult.Failure($"An unexpected error occurred while adding clan with tag {clanTag}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Clan>>> GetAllClansAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all clans from database");
                var data = await _clanRepository.GetAllClansAsync();
                return ServiceResult<IEnumerable<Clan>>.Successful(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving all clans");
                return ServiceResult<IEnumerable<Clan>>.Failure("An unexpected error occurred while retrieving all clans");
            }
        }

        public async Task<ServiceResult<Clan>> GetClanAsync(string clanTag)
        {
            try
            {
                // Input Validation
                var sanitizedTag = ClanTagValidator.ValidateAndSanitizeClanTag(clanTag);
                if (!sanitizedTag.isValid)
                {
                    _logger.LogWarning("Invalid clan tag provided: {ClanTag}", clanTag);
                    return ServiceResult<Clan>.Failure(sanitizedTag.errorMessage);
                }
                var tag = sanitizedTag.sanitizedTag;

                _logger.LogInformation("Retrieving clan with tag {ClanTag} from database", tag);
                var clan = await _clanRepository.GetClanAsync(tag);
                if (clan == null)
                {
                    _logger.LogWarning("Clan with tag {ClanTag} not found in database", tag);
                    return ServiceResult<Clan>.Failure($"Clan with tag '{tag}' not found in database");
                }

                _logger.LogInformation("Successfully retrieved clan {ClanName} with tag {ClanTag}", clan.Name, clan.Tag);
                return ServiceResult<Clan>.Successful(clan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving clan with tag {ClanTag}", clanTag);
                return ServiceResult<Clan>.Failure($"An unexpected error occurred while retrieving clan with tag {clanTag}");
            }
        }

        public async Task<ServiceResult> DeleteClanAsync(string clanTag)
        {
            try
            {
                _logger.LogInformation("Deleting clan with tag {ClanTag}", clanTag);
                if (await _clanRepository.DeleteClanAsync(clanTag))
                {
                    _logger.LogInformation("Successfully deleted clan with tag {ClanTag}", clanTag);
                    return ServiceResult.Successful($"Clan with tag {clanTag} successfully deleted");
                }
                else
                {
                    _logger.LogWarning("Clan with tag {ClanTag} not found in database", clanTag);
                    return ServiceResult.Failure($"Clan with tag {clanTag} not found in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting clan with tag {ClanTag}", clanTag);
                return ServiceResult.Failure($"An unexpected error occurred while deleting clan with tag {clanTag}");
            }
        }

        public async Task<ServiceResult> UpdateClanAsync(string clanTag)
        {
            try
            {
                // Input Validation
                var sanitizedTag = ClanTagValidator.ValidateAndSanitizeClanTag(clanTag);
                if (!sanitizedTag.isValid)
                {
                    _logger.LogWarning("Invalid clan tag provided: {ClanTag}", clanTag);
                    return ServiceResult.Failure(sanitizedTag.errorMessage);
                }

                var tag = sanitizedTag.sanitizedTag;
                _logger.LogInformation("Updating clan with tag {ClanTag}", tag);
                var updatedClan = await _clashRoyaleService.GetClanByTagAsync(tag);

                if (updatedClan == null)
                {
                    _logger.LogWarning("Clan with tag {ClanTag} not found in API", tag);
                    return ServiceResult.Failure($"Clan with tag '{tag}' not found in API");
                }

                updatedClan.Tag = Regex.Replace(updatedClan.Tag, @"[^a-zA-Z0-9]", "");
                _logger.LogInformation("Clan {ClanName} with tag {ClanTag} found. Updating in database", updatedClan.Name, updatedClan.Tag);

                if (await _clanRepository.UpdateClanAsync(updatedClan))
                {
                    _logger.LogInformation("Successfully updated {ClanName} with tag {ClanTag} in database", updatedClan.Name, updatedClan.Tag);
                    return ServiceResult.Successful($"{updatedClan.Name} successfully updated in Clans!");
                }
                else
                {
                    _logger.LogWarning("Clan with tag {ClanTag} does not exist in database", updatedClan.Tag);
                    return ServiceResult.Failure($"Clan with tag '{updatedClan.Tag}' does not exist in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating clan with tag {ClanTag}", clanTag);
                return ServiceResult.Failure($"An unexpected error occurred while updating clan with tag {clanTag}");
            }
        }

        public async Task<ServiceResult> PopulateClanHistoryAsync(Clan clan)
        {
            try
            {
                _logger.LogInformation("Updating history for clan {ClanName}", clan.Name);

                var riverRaceLog = await _clashRoyaleService.GetRiverRaceLogAsync(clan.Tag);
                if (riverRaceLog == null || riverRaceLog.Items == null || riverRaceLog.Items.Count == 0)
                {
                    _logger.LogWarning("No war log data found for clan with tag {ClanTag}", clan.Tag);
                    return ServiceResult.Failure($"No war log data found for clan with tag '{clan.Tag}'");
                }

                var clanHistories = new List<ClanHistory>();
                int runningTrophyDifference = 0;

                foreach (var riverRace in riverRaceLog.Items)
                {
                    // Find the clan's standing in this river race
                    var clanStanding = riverRace.Standings?.FirstOrDefault(s =>
                        s.Clan.Tag.Replace("#", "") == clan.Tag);

                    if (clanStanding != null)
                    {
                        runningTrophyDifference += clanStanding.TrophyChange;

                        var clanHistory = new ClanHistory
                        {
                            ClanID = clan.ID,
                            SeasonID = riverRace.SeasonId,
                            WeekIndex = riverRace.SectionIndex,
                            WarTrophies = clan.WarTrophies - runningTrophyDifference, // take current trophies and subtract difference
                        };

                        clanHistories.Add(clanHistory);
                    }
                }

                if (clanHistories.Count == 0)
                {
                    _logger.LogWarning("No valid clan standings found in war log for clan with tag {ClanTag}", clan.Tag);
                    return ServiceResult.Failure($"No valid clan standings found in war log for clan with tag '{clan.Tag}'");
                }

                if (await _clanRepository.PopulateClanHistoryAsync(clan, clanHistories))
                {
                    _logger.LogInformation("Successfully updated history for {ClanName} in database", clan.Name);
                    return ServiceResult.Successful($"{clan.Name} history successfully updated!");
                }
                else
                {
                    _logger.LogWarning("Failed to update history for clan with tag {ClanTag}", clan.Tag);
                    return ServiceResult.Failure($"Failed to update history for clan with tag '{clan.Tag}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating history for clan with tag {ClanTag}", clan.Tag);
                return ServiceResult.Failure($"An unexpected error occurred while updating history for clan with tag {clan.Tag}");
            }
        }

        public async Task<ServiceResult> PopulatePlayerWarHistories(Clan clan, int numOfRiverRaces = 1) // Looks at every river race past
        {
            try
            {
                _logger.LogInformation("Populating raw war history for {ClanName}", clan.Name);

                var riverRaceLog = await _clashRoyaleService.GetRiverRaceLogAsync(clan.Tag);
                if (riverRaceLog == null || riverRaceLog.Items == null || riverRaceLog.Items.Count == 0)
                {
                    _logger.LogWarning("No war log data found for clan with tag {ClanTag}", clan.Tag);
                    return ServiceResult.Failure($"No war log data found for clan with tag '{clan.Tag}'");
                }

                var playerWarHistories = new List<PlayerWarHistory>();
                var racesToProcess = riverRaceLog.Items.Take(numOfRiverRaces);

                foreach (var riverRace in racesToProcess)
                {
                    // Find the clan's standing in this river race
                    var clanStanding = riverRace.Standings?.FirstOrDefault(s => s.Clan.Tag.Replace("#", "") == clan.Tag);
                    if (clanStanding != null)
                    {
                        var clanHistory = await _clanRepository.GetClanHistoryAsync(clan.ID, riverRace.SeasonId, riverRace.SectionIndex);
                        if (clanHistory == null)
                        {
                            _logger.LogWarning("No clan history found for {ClanName} for Season {SeasonId}, Week {WeekIndex}", clan.Name, riverRace.SeasonId, riverRace.SectionIndex);
                            return ServiceResult.Failure($"No clan history found for {clan.Name} for Season {riverRace.SeasonId}, Week {riverRace.SectionIndex}");
                        }

                        foreach (var participant in clanStanding.Clan.Participants)
                        {
                            if (participant.Fame != 0)
                            {
                                int playerID;
                                var playerTag = participant.Tag.Replace("#", "");
                                var existingPlayer = await _playerRepository.GetPlayerAsync(playerTag);
                                if (existingPlayer == null)
                                {
                                    var newPlayer = new Player
                                    {
                                        Tag = playerTag,
                                        ClanID = clan.ID,
                                        Name = participant.Name,
                                        Status = "Active",
                                    };

                                    playerID = await _playerRepository.AddPlayerAsync(newPlayer);

                                }
                                else
                                {
                                    playerID = existingPlayer.ID;
                                }
                                var playerWarHistory = new PlayerWarHistory
                                {
                                    PlayerID = playerID,
                                    ClanHistoryID = clanHistory.ID,
                                    Fame = participant.Fame,
                                    DecksUsed = participant.DecksUsed,
                                    BoatAttacks = participant.BoatAttacks,
                                    IsModified = false,
                                    UpdatedBy = "System"
                                };
                                playerWarHistories.Add(playerWarHistory);
                            }
                        }
                    }
                }

                if (await _warRepository.AddPlayerWarHistoriesAsync(playerWarHistories))
                {
                    _logger.LogInformation("Successfully populated raw war history for {ClanName}", clan.Name);
                    return ServiceResult.Successful($"{clan.Name} raw war history successfully populated!");
                }
                else
                {
                    _logger.LogWarning("Failed to populate raw war history for clan {ClanName}", clan.Name);
                    return ServiceResult.Failure($"Failed to populate raw war history for clan {clan.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while populating raw war history for {ClanName}", clan.Name);
                return ServiceResult.Failure($"An unexpected error occurred while populating raw war history for {clan.Name}");
            }
        }

        public async Task<ServiceResult> UpdateAllActivePlayerAverages(int numOfWeeksToUse = 4, bool aboveFiveThousandTrophies = true)
        {
            try
            {
                _logger.LogInformation("Updating player averages for all players {TrophyLevel} 5000 trophies", aboveFiveThousandTrophies ? "above" : "below");
                var players = await _playerRepository.GetAllActivePlayersAsync();
                if (players == null || players.Count == 0)
                {
                    _logger.LogWarning("No active players found in database");
                    return ServiceResult.Failure("No active players found in database");
                }

                foreach (var player in players)
                {
                    await UpdatePlayerAverageForTrophyLevelAsync(player, numOfWeeksToUse, aboveFiveThousandTrophies);
                    _logger.LogInformation("Successfully updated player average for {PlayerName} ({PlayerTag})", player.Name, player.Tag);
                }

                _logger.LogInformation("Successfully updated player averages for all players");
                return ServiceResult.Successful("Player averages successfully updated!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating player averages");
                return ServiceResult.Failure($"An unexpected error occurred while updating player averages");
            }
        }

        public async Task<ServiceResult> UpdatePlayerAverageAsync(int playerId, int numOfWeeksToUse = 4)
        {
            try
            {
                _logger.LogInformation("Updating player averages for PlayerID {PlayerId}", playerId);

                var player = await _playerRepository.GetPlayerByIdAsync(playerId);
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found", playerId);
                    return ServiceResult.Failure($"Player with ID {playerId} not found");
                }

                var average5kResult = await UpdatePlayerAverageForTrophyLevelAsync(player, numOfWeeksToUse, true);  // 5k+
                if (!average5kResult.Success)
                {
                    _logger.LogWarning("Failed to update 5k+ average for player {PlayerName} ({PlayerTag}): {ErrorMessage}", player.Name, player.Tag, average5kResult.Message);
                    return ServiceResult.Failure($"Failed to update 5k+ average for player {player.Name}: {average5kResult.Message}");
                }

                var average4kResult = await UpdatePlayerAverageForTrophyLevelAsync(player, numOfWeeksToUse, false); // sub-5k
                if (!average4kResult.Success)
                {
                    _logger.LogWarning("Failed to update sub-5k average for player {PlayerName} ({PlayerTag}): {ErrorMessage}", player.Name, player.Tag, average4kResult.Message);
                    return ServiceResult.Failure($"Failed to update sub-5k average for player {player.Name}: {average4kResult.Message}");
                }

                _logger.LogInformation("Successfully updated player averages for {PlayerName} ({PlayerTag})", player.Name, player.Tag);
                return ServiceResult.Successful($"Player averages updated successfully for {player.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating player averages for PlayerID {PlayerId}", playerId);
                return ServiceResult.Failure($"An unexpected error occurred while updating player averages for PlayerID {playerId}");
            }
        }

        private async Task<ServiceResult> UpdatePlayerAverageForTrophyLevelAsync(Player player, int numOfWeeksToUse, bool aboveFiveThousandTrophies)
        {
            try
            {
                int fame = 0;
                int decksUsed = 0;

                _logger.LogDebug("Grabbing last {NumOfWeeks} weeks of war history for player {PlayerName} ({PlayerTag}) for {TrophyLevel}", numOfWeeksToUse, player.Name, player.Tag, aboveFiveThousandTrophies ? "5k+" : "sub-5k");

                var warHistoriesResult = await _warRepository.GetPlayerWarHistoriesAsync(player, numOfWeeksToUse, aboveFiveThousandTrophies);
                if (warHistoriesResult == null || warHistoriesResult.Count == 0)
                {
                    _logger.LogDebug($"No war history found for player {player.Name} ({player.Tag}) for {(aboveFiveThousandTrophies ? "5k+" : "sub-5k")} trophies");
                    return ServiceResult.Successful($"No war history found for player {player.Name} ({player.Tag}) for {(aboveFiveThousandTrophies ? "5k+" : "sub-5k")} trophies");
                }

                foreach (var warHistory in warHistoriesResult)
                {
                    fame += warHistory.Fame;
                    decksUsed += warHistory.DecksUsed;
                }

                int mostRecentClanID = await _clanRepository.GetMostRecentClanIDAsync(player, aboveFiveThousandTrophies);
                var newPlayerAverage = new PlayerAverage
                {
                    PlayerID = player.ID,
                    ClanID = mostRecentClanID,
                    FameAttackAverage = decksUsed == 0 ? 0 : Math.Round((decimal)fame / decksUsed, 2),
                    Attacks = decksUsed,
                    Is5k = aboveFiveThousandTrophies,
                };

                await _playerRepository.UpsertPlayerAverageAsync(newPlayerAverage);

                _logger.LogDebug("Successfully updated {TrophyLevel} player average for {PlayerName} ({PlayerTag})", aboveFiveThousandTrophies ? "5k+" : "sub-5k", player.Name, player.Tag);
                return ServiceResult.Successful($"{(aboveFiveThousandTrophies ? "5k+" : "sub-5k")} player average updated successfully for {player.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {TrophyLevel} average for player {PlayerName} ({PlayerTag})", aboveFiveThousandTrophies ? "5k+" : "sub-5k", player.Name, player.Tag);
                return ServiceResult.Failure($"Error updating {(aboveFiveThousandTrophies ? "5k+" : "sub-5k")} average for player {player.Name}: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<PlayerAverageDTO>>> GetAllPlayerAverageDTOsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all player averages from database");
                var data = await _playerRepository.GetAllPlayerAverageDTOsAsync();
                return ServiceResult<IEnumerable<PlayerAverageDTO>>.Successful(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving all player averages");
                return ServiceResult<IEnumerable<PlayerAverageDTO>>.Failure("An unexpected error occurred while retrieving all player averages");
            }
        }

        public async Task<ServiceResult<IEnumerable<GroupedPlayerWarHistoryDTO>>> GetAllGroupedPlayerWarHistoryDTOsAsync(bool is5k = true)
        {
            try
            {
                _logger.LogInformation("Retrieving grouped player war histories for {TrophyLevel} trophies", is5k ? "5k+" : "sub-5k");

                var allPlayerWarHistories = await _warRepository.GetAllPlayerWarHistoriesExpandedAsync(is5k);
                
                if (allPlayerWarHistories == null || !allPlayerWarHistories.Any())
                {
                    _logger.LogWarning("No player war histories found for {TrophyLevel} trophies", is5k ? "5k+" : "sub-5k");
                    return ServiceResult<IEnumerable<GroupedPlayerWarHistoryDTO>>.Successful(new List<GroupedPlayerWarHistoryDTO>());
                }

                // Clean clan tags
                foreach (var pwh in allPlayerWarHistories)
                {
                    pwh.PlayerTag = Regex.Replace(pwh.PlayerTag, @"[^a-zA-Z0-9]", "");
                }

                // Group by PlayerID, SeasonID, and WeekIndex, then aggregate
                var groupedResults = allPlayerWarHistories
                    .GroupBy(pwh => new { pwh.PlayerID, pwh.SeasonID, pwh.WeekIndex })
                    .Select(group => new GroupedPlayerWarHistoryDTO
                    {
                        PlayerWarHistoryIDs = group.Select(g => g.ID).ToList(),
                        PlayerID = group.Key.PlayerID,
                        PlayerTag = group.First().PlayerTag,
                        PlayerName = group.First().PlayerName,
                        Status = group.First().Status,
                        Notes = group.First().Notes,
                        SeasonID = group.Key.SeasonID,
                        WeekIndex = group.Key.WeekIndex,
                        Fame = group.Sum(g => g.Fame), // Aggregate fame
                        DecksUsed = group.Sum(g => g.DecksUsed + g.BoatAttacks), // Aggregate decks used (attacks)
                        ClanID = group.First().ClanID, // Most recent clan
                        ClanName = group.First().ClanName, // Most recent clan name
                        LastUpdated = group.Max(g => g.LastUpdated) // Latest update time
                    })
                    .OrderBy(dto => dto.PlayerName)
                    .ThenByDescending(dto => dto.SeasonID)
                    .ThenByDescending(dto => dto.WeekIndex)
                    .ToList();

                _logger.LogInformation("Successfully grouped {OriginalCount} player war histories into {GroupedCount} grouped records for {TrophyLevel} trophies", 
                    allPlayerWarHistories.Count, groupedResults.Count, is5k ? "5k+" : "sub-5k");

                return ServiceResult<IEnumerable<GroupedPlayerWarHistoryDTO>>.Successful(groupedResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving grouped player war histories for {TrophyLevel} trophies", is5k ? "5k+" : "sub-5k");
                return ServiceResult<IEnumerable<GroupedPlayerWarHistoryDTO>>.Failure($"An unexpected error occurred while retrieving grouped player war histories for {(is5k ? "5k+" : "sub-5k")} trophies");
            }
        }

        public async Task<ServiceResult<Player>> GetPlayerByIdAsync(int playerId)
        {
            try
            {
                _logger.LogInformation("Getting player by ID: {PlayerId}", playerId);

                var player = await _playerRepository.GetPlayerByIdAsync(playerId);
                if (player == null)
                {
                    return ServiceResult<Player>.Failure($"Player with ID {playerId} not found");
                }

                return ServiceResult<Player>.Successful(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player by ID: {PlayerId}", playerId);
                return ServiceResult<Player>.Failure($"Error retrieving player: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdatePlayerStatusAsync(int playerId, string status)
        {
            try
            {
                _logger.LogInformation("Updating player status: PlayerID {PlayerId} to {Status}", playerId, status);

                var success = await _playerRepository.UpdatePlayerStatusAsync(playerId, status);
                if (!success)
                {
                    return ServiceResult.Failure($"Player with ID {playerId} not found");
                }

                return ServiceResult.Successful("Player status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player status: PlayerID {PlayerId}", playerId);
                return ServiceResult.Failure($"Error updating player status: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<PlayerWarHistoryExpanded>>> GetPlayerWarHistoriesByPlayerIdAsync(int playerId)
        {
            try
            {
                _logger.LogInformation("Getting war histories for PlayerID: {PlayerId}", playerId);

                var warHistories = await _warRepository.GetPlayerWarHistoriesByPlayerIdAsync(playerId);

                return ServiceResult<IEnumerable<PlayerWarHistoryExpanded>>.Successful(warHistories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting war histories for PlayerID: {PlayerId}", playerId);
                return ServiceResult<IEnumerable<PlayerWarHistoryExpanded>>.Failure($"Error retrieving war histories: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdatePlayerWarHistoryAsync(int warHistoryId, int fame, int decksUsed, int boatAttacks)
        {
            try
            {
                _logger.LogInformation("Updating war history: ID {WarHistoryId}", warHistoryId);

                // Validate inputs
                if (fame < 0 || decksUsed < 0 || boatAttacks < 0)
                {
                    return ServiceResult.Failure("Fame, decks used, and boat attacks must be non-negative values");
                }

                if (decksUsed > 16 || fame > 3600 || boatAttacks > 16)
                {
                    return ServiceResult.Failure("Invalid input for decksUsed, fame, or boatAttacks");
                }

                var updateSuccess = await _warRepository.UpdatePlayerWarHistoryAsync(warHistoryId, fame, decksUsed, boatAttacks);
                if (!updateSuccess)
                {
                    return ServiceResult.Failure($"War history with ID {warHistoryId} not found");
                }

                return ServiceResult.Successful("War history updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating war history: ID {WarHistoryId}", warHistoryId);
                return ServiceResult.Failure($"Error updating war history: {ex.Message}");
            }
        }

        public async Task<ServiceResult> AddClanClanHistoryPlayerHistoryAsync(string clanTag, int numWeeksWarHistory)
        {
            try
            {
                var addClanResult = await AddClanAsync(clanTag);
                if (!addClanResult.Success)
                {
                    return addClanResult; // Return failure reason from AddClanAsync
                }

                var getClanResult = await GetClanAsync(clanTag);
                if (!getClanResult.Success || getClanResult.Data == null)
                {
                    return ServiceResult.Failure($"Failed to retrieve clan after adding: {getClanResult.Message}");
                }

                var populateClanHistoryResult = await PopulateClanHistoryAsync(getClanResult.Data);
                if (!populateClanHistoryResult.Success)
                {
                    return ServiceResult.Failure($"Failed to populate clan history: {populateClanHistoryResult.Message}");
                }

                var populatePlayerWarHistoriesResult = await PopulatePlayerWarHistories(getClanResult.Data, numWeeksWarHistory);
                if (!populatePlayerWarHistoriesResult.Success)
                {
                    return ServiceResult.Failure($"Failed to populate player war histories: {populatePlayerWarHistoriesResult.Message}");
                }

                return ServiceResult.Successful($"Successfully added clan, populated clan history, and populated player war histories for clan with tag {clanTag}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding clan with history for tag {ClanTag}", clanTag);
                return ServiceResult.Failure($"An unexpected error occurred while adding clan with history for tag {clanTag}");
            }
        }

        public async Task<ServiceResult<int>> GetPlayerIdFromWarHistoryAsync(int warHistoryId)
        {
            try
            {
                _logger.LogDebug("Getting PlayerID for war history ID {WarHistoryId}", warHistoryId);

                var playerId = await _warRepository.GetPlayerIdFromWarHistoryAsync(warHistoryId);
                if (!playerId.HasValue)
                {
                    return ServiceResult<int>.Failure($"War history with ID {warHistoryId} not found");
                }

                return ServiceResult<int>.Successful(playerId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PlayerID for war history ID {WarHistoryId}", warHistoryId);
                return ServiceResult<int>.Failure($"Error getting PlayerID for war history ID {warHistoryId}");
            }
        }

        public async Task<ServiceResult> UpdatePlayerNotesAsync(int playerId, string? notes)
        {
            try
            {
                if (string.IsNullOrEmpty(notes))
                {
                    notes = null;
                }
                else if (notes.Length > 100)
                {
                    return ServiceResult.Failure("Notes must be 100 characters or fewer.");
                }

                var success = await _playerRepository.UpdatePlayerNotesAsync(playerId, notes);
                if (!success)
                {
                    return ServiceResult.Failure($"Player with ID {playerId} not found");
                }

                return ServiceResult.Successful("Player notes updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes for PlayerID {PlayerId}", playerId);
                return ServiceResult.Failure($"Error updating notes: {ex.Message}");
            }
        }
        public async Task<ServiceResult<IEnumerable<RosterAssignmentDTO>>> GetAllRosterAssignmentDTOsAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving roster assignments DTOs from repository");
                var data = await _playerRepository.GetAllRosterAssignmentDTOsAsync();
                return ServiceResult<IEnumerable<RosterAssignmentDTO>>.Successful(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve roster assignments DTOs");
                return ServiceResult<IEnumerable<RosterAssignmentDTO>>.Failure("An unexpected error occurred while retrieving roster assignments");
            }
        }

        public async Task<ServiceResult> UpdateRosterByFameAverageAsync()
        {
            try
            {
                _logger.LogInformation("Updating roster assignments based on fame averages");

                // Get 5k+ and sub-5k player averages for ACTIVE players, combine and deduplicate
                var fiveKResult = await GetAllActivePlayerAveragesAsync(true);
                if (!fiveKResult.Success || fiveKResult.Data == null)
                {
                    _logger.LogWarning("Failed to retrieve 5k+ player averages: {Message}", fiveKResult.Message);
                    return ServiceResult.Failure($"Failed to retrieve 5k+ player averages: {fiveKResult.Message}");
                }

                var subFiveKResult = await GetAllActivePlayerAveragesAsync(false);
                if (!subFiveKResult.Success || subFiveKResult.Data == null)
                {
                    _logger.LogWarning("Failed to retrieve sub-5k player averages: {Message}", subFiveKResult.Message);
                    return ServiceResult.Failure($"Failed to retrieve sub-5k player averages: {subFiveKResult.Message}");
                }

                var fiveKPlayers = fiveKResult.Data.ToList();
                var subFiveKPlayers = subFiveKResult.Data.ToList();

                _logger.LogInformation("Retrieved {FiveKCount} 5k+ active players and {SubFiveKCount} sub-5k active players",
                    fiveKPlayers.Count, subFiveKPlayers.Count);

                // Remove duplicates from sub-5k list (prioritize 5k+ list)
                var fiveKPlayerIds = fiveKPlayers.Select(p => p.PlayerID).ToHashSet();
                var duplicatePlayerIds = subFiveKPlayers
                    .Where(p => fiveKPlayerIds.Contains(p.PlayerID))
                    .Select(p => p.PlayerID)
                    .ToList();

                if (duplicatePlayerIds.Any())
                {
                    _logger.LogInformation("Found {DuplicateCount} players in both lists. Removing from sub-5k list: {PlayerIds}",
                        duplicatePlayerIds.Count, string.Join(", ", duplicatePlayerIds));

                    subFiveKPlayers = subFiveKPlayers
                        .Where(p => !fiveKPlayerIds.Contains(p.PlayerID))
                        .ToList();
                }

                // Combine and sort ACTIVE players by fame attack average (descending)
                var allActivePlayerAverages = fiveKPlayers
                    .Concat(subFiveKPlayers)
                    .OrderByDescending(p => p.Is5k) // 5k+ players first (true > false)
                    .ThenByDescending(p => p.FameAttackAverage) // Then by fame average descending
                    .ToList();

                _logger.LogInformation("Combined and sorted {TotalCount} active players by fame average", allActivePlayerAverages.Count);

                // Get all L2W (Left 2 Weeks) players to assign to unassigned section
                var l2wPlayers = await _playerRepository.GetL2WPlayersAsync();

                _logger.LogInformation("Found {L2WCount} L2W players to assign to unassigned", l2wPlayers.Count);

                var clansResult = await GetAllClansAsync();
                if (!clansResult.Success || clansResult.Data == null)
                {
                    _logger.LogWarning("Failed to retrieve clans: {Message}", clansResult.Message);
                    return ServiceResult.Failure($"Failed to retrieve clans: {clansResult.Message}");
                }

                var clans = clansResult.Data
                    .OrderByDescending(c => c.WarTrophies) // Sort by war trophies descending for assignment
                    .ToList();
                _logger.LogInformation("Found {ClanCount} clans for roster assignment", clans.Count);

                if (clans.Count == 0)
                {
                    return ServiceResult.Failure("No clans available for roster assignment");
                }

                var rosterAssignments = new List<RosterAssignment>();
                var currentClanIndex = 0;
                var playersPerClan = 50;
                var playersInCurrentClan = 0;
                var totalClanCapacity = clans.Count * playersPerClan;
                var activePlayersUnassignedCount = 0;
                var l2wPlayersUnassignedCount = 0;

                // First, assign ACTIVE players to clans
                foreach (var playerAverage in allActivePlayerAverages)
                {
                    RosterAssignment rosterAssignment;

                    if (rosterAssignments.Count >= totalClanCapacity)
                    {
                        // Assign remaining active players to ClanID = null (overflow)
                        rosterAssignment = new RosterAssignment
                        {
                            SeasonID = 999, // Current season placeholder
                            WeekIndex = 999, // Current week placeholder
                            PlayerID = playerAverage.PlayerID,
                            ClanID = null, // Unassigned
                            IsInClan = false,
                            UpdatedBy = "AutoRoster-Overflow"
                        };

                        activePlayersUnassignedCount++;
                        _logger.LogDebug("Assigned active PlayerID {PlayerId} to unassigned (excess capacity)", playerAverage.PlayerID);
                    }
                    else
                    {
                        // Move to next clan if current clan is full
                        if (playersInCurrentClan >= playersPerClan && currentClanIndex < clans.Count - 1)
                        {
                            currentClanIndex++;
                            playersInCurrentClan = 0;
                        }

                        var assignedClan = clans[currentClanIndex];

                        rosterAssignment = new RosterAssignment
                        {
                            SeasonID = 999, // Current season placeholder
                            WeekIndex = 999, // Current week placeholder
                            PlayerID = playerAverage.PlayerID,
                            ClanID = assignedClan.ID,
                            IsInClan = false,
                            UpdatedBy = "AutoRoster"
                        };

                        playersInCurrentClan++;
                        _logger.LogDebug("Assigned active PlayerID {PlayerId} to {ClanName} ({PlayersInClan}/{MaxPlayers})",
                            playerAverage.PlayerID, assignedClan.Name, playersInCurrentClan, playersPerClan);
                    }

                    rosterAssignments.Add(rosterAssignment);
                }

                // Second, assign all L2W players to unassigned (ClanID = null)
                foreach (var l2wPlayer in l2wPlayers)
                {
                    var rosterAssignment = new RosterAssignment
                    {
                        SeasonID = 999,
                        WeekIndex = 999,
                        PlayerID = l2wPlayer.ID,
                        ClanID = null, // Unassigned
                        IsInClan = false,
                        UpdatedBy = "AutoRoster-L2W"
                    };

                    rosterAssignments.Add(rosterAssignment);
                    l2wPlayersUnassignedCount++;
                    _logger.LogDebug("Assigned L2W PlayerID {PlayerId} to unassigned",
                        l2wPlayer.ID);
                }

                var bulkUpsertResult = await _playerRepository.BulkUpsertRosterAssignmentsAsync(rosterAssignments);
                if (!bulkUpsertResult)
                {
                    return ServiceResult.Failure("Failed to save roster assignments to database");
                }

                var assignedToClanCount = rosterAssignments.Count - activePlayersUnassignedCount - l2wPlayersUnassignedCount;
                var totalUnassignedCount = activePlayersUnassignedCount + l2wPlayersUnassignedCount;

                _logger.LogInformation("Successfully created {Count} roster assignments: {AssignedCount} assigned to {ClanCount} clans, {TotalUnassigned} unassigned ({ActiveUnassigned} active overflow, {L2WUnassigned} L2W)",
                    rosterAssignments.Count, assignedToClanCount, clans.Count, totalUnassignedCount, activePlayersUnassignedCount, l2wPlayersUnassignedCount);

                var message = $"Successfully created {rosterAssignments.Count} roster assignments for season 999, week 999";
                if (totalUnassignedCount > 0)
                {
                    message += $" ({assignedToClanCount} assigned to clans, {totalUnassignedCount} unassigned: {activePlayersUnassignedCount} active overflow, {l2wPlayersUnassignedCount} L2W)";
                }

                return ServiceResult.Successful(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating roster by fame average");
                return ServiceResult.Failure("An unexpected error occurred while updating roster by fame average");
            }
        }

        public async Task<ServiceResult> UpdateRosterAssignmentAsync(int rosterAssignmentId, int? assignedClanId)
        {
            try
            {
                _logger.LogInformation("Updating roster assignment {Id} to ClanID {ClanId}", rosterAssignmentId, assignedClanId);

                // Update the roster assignment directly by ID
                var success = await _playerRepository.UpdateRosterAssignmentClanAsync(rosterAssignmentId, assignedClanId);

                if (success)
                {
                    _logger.LogInformation("Successfully updated roster assignment {Id} to ClanID {ClanId}", rosterAssignmentId, assignedClanId);
                    return ServiceResult.Successful("Roster assignment updated successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to update roster assignment {Id} - roster assignment not found", rosterAssignmentId);
                    return ServiceResult.Failure($"Roster assignment with ID {rosterAssignmentId} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating roster assignment {Id}", rosterAssignmentId);
                return ServiceResult.Failure($"An unexpected error occurred while updating roster assignment: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateRosterInClanStatusAsync()
        {
            try
            {
                _logger.LogInformation("Starting roster IsInClan status update for all clans");

                var clansResult = await GetAllClansAsync();
                if (!clansResult.Success || clansResult.Data == null)
                {
                    _logger.LogWarning("Failed to retrieve clans for IsInClan status update: {Message}", clansResult.Message);
                    return ServiceResult.Failure($"Failed to retrieve clans for IsInClan status update: {clansResult.Message}");
                }

                var clans = clansResult.Data.ToList();
                _logger.LogInformation("Found {ClanCount} clans to update IsInClan status for", clans.Count);

                // Get unique clan IDs from roster assignments by calling a lightweight method
                // We'll determine unique clans by calling the clan-specific method with each clan ID
                var uniqueClanIds = new List<int?>();
                
                // Add all existing clan IDs
                uniqueClanIds.AddRange(clans.Select(c => (int?)c.ID));
                
                // Add null for unassigned players
                uniqueClanIds.Add(null);

                _logger.LogInformation("Found {UniqueClansCount} unique clan assignments to process", uniqueClanIds.Count);

                var overallSuccess = true;
                var clanResults = new List<(int? ClanId, ServiceResult Result)>();
                var totalUpdatedCount = 0;
                var totalProcessedCount = 0;

                // Process each clan separately
                foreach (var clanId in uniqueClanIds)
                {
                    var clanName = clanId.HasValue 
                        ? clans.FirstOrDefault(c => c.ID == clanId)?.Name ?? $"ClanID {clanId}" 
                        : "Unassigned";

                    _logger.LogInformation("Processing IsInClan status update for {ClanName} (ClanID: {ClanId})", clanName, clanId);

                    var result = await UpdateRosterInClanStatusForClanAsync(clanId);
                    clanResults.Add((clanId, result));

                    if (result.Success)
                    {
                        _logger.LogInformation("Successfully completed IsInClan update for {ClanName}: {Message}", clanName, result.Message);
                        
                        // Extract counts from the result message for aggregation
                        if (result.Message.Contains("Total: "))
                        {
                            // Parse the counts from the message (format: "Total: X, Updated: Y, ...")
                            var parts = result.Message.Split(',').Select(p => p.Trim()).ToArray();
                            foreach (var part in parts)
                            {
                                if (part.StartsWith("Total: "))
                                {
                                    if (int.TryParse(part.Substring("Total: ".Length), out var count))
                                        totalProcessedCount += count;
                                }
                                else if (part.StartsWith("Updated: "))
                                {
                                    var updatedPart = part.Substring("Updated: ".Length);
                                    // Handle format "Updated: X (Assigned: Y, Unassigned: Z)"
                                    var spaceIndex = updatedPart.IndexOf(' ');
                                    var countStr = spaceIndex > 0 ? updatedPart.Substring(0, spaceIndex) : updatedPart;
                                    if (int.TryParse(countStr, out var count))
                                        totalUpdatedCount += count;
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed IsInClan update for {ClanName}: {Message}", clanName, result.Message);
                        overallSuccess = false;
                    }
                }

                // Compile overall results
                var successfulClans = clanResults.Count(r => r.Result.Success);
                var failedClans = clanResults.Count(r => !r.Result.Success);

                var summaryMessage = $"IsInClan status update completed for all clans. " +
                                   $"Clans processed: {uniqueClanIds.Count}, " +
                                   $"Successful: {successfulClans}, " +
                                   $"Failed: {failedClans}, " +
                                   $"Total players processed: {totalProcessedCount}, " +
                                   $"Total players updated: {totalUpdatedCount}";

                _logger.LogInformation(summaryMessage);

                if (failedClans == 0)
                {
                    return ServiceResult.Successful(summaryMessage);
                }
                else if (successfulClans > 0)
                {
                    return ServiceResult.Successful($"Partial success: {summaryMessage}");
                }
                else
                {
                    return ServiceResult.Failure($"All clan updates failed: {summaryMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during overall roster IsInClan status update");
                return ServiceResult.Failure("An unexpected error occurred during overall roster IsInClan status update");
            }
        }

        public async Task<ServiceResult> UpdateRosterInClanStatusForClanAsync(int? clanId)
        {
            try
            {
                var clanName = clanId.HasValue ? "specific clan" : "unassigned players";
                _logger.LogInformation("Starting roster IsInClan status update for {ClanName} (ClanID: {ClanId})", clanName, clanId);

                // Get roster assignments for the specific clan only
                var rosterAssignments = await _playerRepository.GetRosterAssignmentsForOneWeekOneClanAsync(999, 999, clanId);

                if (!rosterAssignments.Any())
                {
                    var msg = clanId.HasValue
                        ? $"No roster assignments found for ClanID {clanId}"
                        : "No unassigned roster assignments found";
                    _logger.LogInformation(msg);
                    return ServiceResult.Successful(msg);
                }

                _logger.LogInformation("Found {Count} roster assignments to check for InClan status for {ClanName}",
                    rosterAssignments.Count, clanName);

                var successfulUpdates = 0;
                var failedUpdates = 0;
                var skippedUpdates = 0;
                var unassignedUpdates = 0;

                foreach (var assignment in rosterAssignments)
                {
                    try
                    {
                        bool shouldBeInClan = false;

                        // Check if player has no assigned clan (ClanID is null)
                        if (!assignment.ClanID.HasValue)
                        {
                            // Player is unassigned - immediately set to false without API call
                            shouldBeInClan = false;
                            _logger.LogDebug("Player {PlayerName} ({PlayerTag}) has no assigned clan, setting IsInClan to false",
                                assignment.PlayerName, assignment.PlayerTag);

                            // Update if status has changed
                            if (assignment.IsInClan != shouldBeInClan)
                            {
                                var updateResult = await _playerRepository.UpdateRosterAssignmentInClanStatusAsync(assignment.ID, shouldBeInClan);

                                if (updateResult)
                                {
                                    unassignedUpdates++;
                                    _logger.LogDebug("Updated IsInClan status for unassigned player {PlayerName} ({PlayerTag}) from {OldStatus} to {NewStatus}",
                                        assignment.PlayerName, assignment.PlayerTag, assignment.IsInClan, shouldBeInClan);
                                }
                                else
                                {
                                    failedUpdates++;
                                    _logger.LogWarning("Failed to update IsInClan status for unassigned player {PlayerName} ({PlayerTag})",
                                        assignment.PlayerName, assignment.PlayerTag);
                                }
                            }
                            else
                            {
                                skippedUpdates++;
                                _logger.LogDebug("IsInClan status already correct for unassigned player {PlayerName} ({PlayerTag}): {Status}",
                                    assignment.PlayerName, assignment.PlayerTag, assignment.IsInClan);
                            }
                        }
                        else
                        {
                            // Player has an assigned clan - make API call to check current clan
                            var playerInfo = await _clashRoyaleService.GetPlayerByTagAsync(assignment.PlayerTag);

                            if (playerInfo == null)
                            {
                                _logger.LogWarning("Could not retrieve player info for {PlayerTag}", assignment.PlayerTag);
                                skippedUpdates++;
                                continue;
                            }

                            // Check if their current clan matches assigned clan
                            shouldBeInClan = !string.IsNullOrEmpty(playerInfo.CurrentClanTag) &&
                                           !string.IsNullOrEmpty(assignment.ClanTag) &&
                                           string.Equals(playerInfo.CurrentClanTag, assignment.ClanTag, StringComparison.OrdinalIgnoreCase);

                            // Update if status has changed
                            if (assignment.IsInClan != shouldBeInClan)
                            {
                                var updateResult = await _playerRepository.UpdateRosterAssignmentInClanStatusAsync(assignment.ID, shouldBeInClan);

                                if (updateResult)
                                {
                                    successfulUpdates++;
                                    _logger.LogDebug("Updated IsInClan status for {PlayerName} ({PlayerTag}) from {OldStatus} to {NewStatus}",
                                        assignment.PlayerName, assignment.PlayerTag, assignment.IsInClan, shouldBeInClan);
                                }
                                else
                                {
                                    failedUpdates++;
                                    _logger.LogWarning("Failed to update IsInClan status for {PlayerName} ({PlayerTag})",
                                        assignment.PlayerName, assignment.PlayerTag);
                                }
                            }
                            else
                            {
                                skippedUpdates++;
                                _logger.LogDebug("IsInClan status already correct for {PlayerName} ({PlayerTag}): {Status}",
                                    assignment.PlayerName, assignment.PlayerTag, assignment.IsInClan);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing roster assignment for {PlayerName} ({PlayerTag})",
                            assignment.PlayerName, assignment.PlayerTag);
                        failedUpdates++;
                    }
                }

                var totalUpdates = successfulUpdates + unassignedUpdates;
                var message = $"IsInClan status update completed for {clanName}. " +
                             $"Total: {rosterAssignments.Count}, " +
                             $"Updated: {totalUpdates} (Assigned: {successfulUpdates}, Unassigned: {unassignedUpdates}), " +
                             $"Failed: {failedUpdates}, " +
                             $"Skipped: {skippedUpdates}";

                _logger.LogInformation(message);

                if (failedUpdates == 0)
                {
                    return ServiceResult.Successful(message);
                }
                else if (totalUpdates > 0)
                {
                    return ServiceResult.Successful($"Partial success: {message}");
                }
                else
                {
                    return ServiceResult.Failure($"All updates failed: {message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during clan-specific roster IsInClan status update for ClanID {ClanId}", clanId);
                return ServiceResult.Failure($"An unexpected error occurred during clan-specific roster IsInClan status update for ClanID {clanId}");
            }
        }

        public async Task<ServiceResult> BackupCurrentRosterToNewSeasonWeekAsync(int newSeasonId, int newWeekIndex)
        {
            try
            {
                _logger.LogInformation("Backing up current roster (Season 999, Week 999) to Season {NewSeasonId}, Week {NewWeekIndex}",
                    newSeasonId, newWeekIndex);

                // Check if roster backup already exists for this season/week
                var existingBackup = await _playerRepository.GetRosterAssignmentsForOneWeekOneClanAsync(newSeasonId, newWeekIndex, null);
                if (existingBackup != null && existingBackup.Any())
                {
                    _logger.LogInformation("Roster backup already exists for Season {SeasonId}, Week {WeekIndex} ({Count} assignments). Skipping backup.",
                        newSeasonId, newWeekIndex, existingBackup.Count);
                    return ServiceResult.Successful($"Roster backup already exists for Season {newSeasonId}, Week {newWeekIndex}");
                }

                // Copy roster assignments from 999, 999 to the new season/week
                var copyResult = await _playerRepository.CopyRosterAssignmentsToNewSeasonWeekAsync(999, 999, newSeasonId, newWeekIndex);
                
                if (copyResult)
                {
                    _logger.LogInformation("Successfully backed up current roster to Season {SeasonId}, Week {WeekIndex}",
                        newSeasonId, newWeekIndex);
                    return ServiceResult.Successful($"Successfully backed up current roster to Season {newSeasonId}, Week {newWeekIndex}");
                }
                else
                {
                    _logger.LogWarning("Failed to backup current roster to Season {SeasonId}, Week {WeekIndex}",
                        newSeasonId, newWeekIndex);
                    return ServiceResult.Failure($"Failed to backup current roster to Season {newSeasonId}, Week {newWeekIndex}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while backing up current roster to Season {SeasonId}, Week {WeekIndex}",
                    newSeasonId, newWeekIndex);
                return ServiceResult.Failure($"An unexpected error occurred while backing up current roster to Season {newSeasonId}, Week {newWeekIndex}");
            }
        }

        public async Task<ServiceResult<IEnumerable<(int SeasonId, int WeekIndex)>>> GetAvailableRosterSeasonWeeksAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving available roster season/weeks");
                var seasonWeeks = await _playerRepository.GetDistinctRosterSeasonWeeksAsync();
                return ServiceResult<IEnumerable<(int SeasonId, int WeekIndex)>>.Successful(seasonWeeks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving available roster season/weeks");
                return ServiceResult<IEnumerable<(int SeasonId, int WeekIndex)>>.Failure("An unexpected error occurred while retrieving available roster season/weeks");
            }
        }

        public async Task<ServiceResult<IEnumerable<RosterAssignmentDTO>>> GetRosterAssignmentsBySeasonWeekAsync(int seasonId, int weekIndex)
        {
            try
            {
                _logger.LogDebug("Retrieving roster assignments for Season {SeasonId}, Week {WeekIndex}", seasonId, weekIndex);
                var rosters = await _playerRepository.GetRosterAssignmentsBySeasonWeekAsync(seasonId, weekIndex);
                return ServiceResult<IEnumerable<RosterAssignmentDTO>>.Successful(rosters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving roster assignments for Season {SeasonId}, Week {WeekIndex}", 
                    seasonId, weekIndex);
                return ServiceResult<IEnumerable<RosterAssignmentDTO>>.Failure($"An unexpected error occurred while retrieving roster assignments for Season {seasonId}, Week {weekIndex}");
            }
        }

        public async Task<ServiceResult<IEnumerable<PlayerAverage>>> GetAllActivePlayerAveragesAsync(bool is5k)
        {
            try
            {
                _logger.LogInformation("Retrieving all active player averages for {TrophyLevel}", is5k ? "5k+" : "sub-5k");
                var playerAverages = await _playerRepository.GetAllActivePlayerAveragesAsync(is5k);
                return ServiceResult<IEnumerable<PlayerAverage>>.Successful(playerAverages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving active player averages for {TrophyLevel}", is5k ? "5k+" : "sub-5k");
                return ServiceResult<IEnumerable<PlayerAverage>>.Failure($"An unexpected error occurred while retrieving active player averages for {(is5k ? "5k+" : "sub-5k")}" );
            }
        }

        public async Task<ServiceResult> UpdateAllPlayerAveragesAsync(int numOfWeeksToUse, bool aboveFiveThousandTrophies)
        {
            try
            {
                _logger.LogInformation("Updating player averages for all players {TrophyLevel} 5000 trophies", aboveFiveThousandTrophies ? "above" : "below");
                var players = await _playerRepository.GetAllPlayersAsync();
                if (players == null || players.Count == 0)
                {
                    _logger.LogWarning("No players found in database");
                    return ServiceResult.Failure("No players found in database");
                }

                foreach (var player in players)
                {
                    await UpdatePlayerAverageForTrophyLevelAsync(player, numOfWeeksToUse, aboveFiveThousandTrophies);
                    _logger.LogInformation("Successfully updated player average for {PlayerName} ({PlayerTag})", player.Name, player.Tag);
                }

                _logger.LogInformation("Successfully updated player averages for all players");
                return ServiceResult.Successful("Player averages successfully updated!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating player averages");
                return ServiceResult.Failure($"An unexpected error occurred while updating player averages");
            }
        }
    }
}