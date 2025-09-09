using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using ClashRoyaleWarTracker.Application.Helpers;

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

        public async Task<ServiceResult> WeeklyUpdateAsync()
        {
            try
            {
                _logger.LogInformation("Starting weekly update for all clans");

                var getAllClansResult = await GetAllClansAsync();
                if (!getAllClansResult.Success || getAllClansResult.Data == null || !getAllClansResult.Data.Any())
                {
                    _logger.LogWarning("Failed to retrieve clans for weekly update");
                    return ServiceResult.Failure("Failed to retrieve clans for weekly update");
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
                    var warHistoryResult = await PopulatePlayerWarHistories(clan); // does last week
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

                // Update all active player averages for 5k+
                var update5kAveragesResult = await UpdateAllActivePlayerAverages(4, true);
                if (update5kAveragesResult.Success)
                {
                    _logger.LogInformation("Successfully updated 5k+ player averages");
                }
                else
                {
                    _logger.LogWarning("Failed to update 5k+ player averages: {ErrorMessage}", update5kAveragesResult.Message);
                }

                // Update all active player averages for sub-5k
                var updateSub5kAveragesResult = await UpdateAllActivePlayerAverages(4, false);
                if (updateSub5kAveragesResult.Success)
                {
                    _logger.LogInformation("Successfully updated sub-5k player averages");
                }
                else
                {
                    _logger.LogWarning("Failed to update sub-5k player averages: {ErrorMessage}", updateSub5kAveragesResult.Message);
                }

                string summary = $"Weekly update completed. " + 
                              $"Total Clans: {totalClans}, " +
                              $"Successful Clan Updates: {successfulUpdates}, Failed Clan Updates: {failedUpdates}, " +
                              $"Successful ClanHistory Updates: {successfulHistoryUpdates}, Failed ClanHistory Updates: {failedHistoryUpdates}," +
                              $"Successful PlayerWarHistory Updates: {successfulWarHistoryUpdates}, Failed PlayerWarHistory Updates: {failedWarHistoryUpdates}";

                _logger.LogInformation("Weekly update completed. Total Clans: {TotalClans}, Successful Clan Updates: {SuccessfulUpdates}, Failed Clan Updates: {FailedUpdates}, Successful ClanHistory Updates: {SuccessfulHistoryUpdates}, Failed ClanHistory Updates: {FailedHistoryUpdates}, Successful PlayerWarHistory Updates: {SuccessfulWarHistoryUpdates}, Failed PlayerWarHistory Updates: {FailedWarHistoryUpdates}", 
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
                _logger.LogError(ex, "An unexpected error occurred during the weekly update");
                return ServiceResult.Failure("An unexpected error occurred during the weekly update");
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
                            RecordedDate = DateTime.Now
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
                                        IsActive = true,
                                        LastUpdated = DateTime.Now
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
                    int fame = 0;
                    int decksUsed = 0;
                    _logger.LogDebug("Grabbing last {NumOfWeeks} weeks of war history for player {PlayerName} ({PlayerTag})", numOfWeeksToUse, player.Name, player.Tag);
                    var warHistoriesResult = await _warRepository.GetPlayerWarHistoriesAsync(player, numOfWeeksToUse, aboveFiveThousandTrophies); // As a reminder, this will not grab records with boat attacks
                    if (warHistoriesResult == null || warHistoriesResult.Count == 0)
                    {
                        _logger.LogDebug("No war history found for player {PlayerName} ({PlayerTag})", player.Name, player.Tag);
                        continue;
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
                        Is5k = aboveFiveThousandTrophies,
                    };

                    await _playerRepository.UpsertPlayerAverageAsync(newPlayerAverage);
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