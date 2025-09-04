using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using ClashRoyaleProject.Application.Helpers;

namespace ClashRoyaleProject.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IClashRoyaleService _clashRoyaleService;
        private readonly IClanRepository _clanRepository;
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(IClashRoyaleService clashRoyaleService, IClanRepository clanRepository, ILogger<ApplicationService> logger)
        {
            _clashRoyaleService = clashRoyaleService;
            _clanRepository = clanRepository;
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

                _logger.LogInformation($"Found {totalClans} clans to update");

                foreach (var clan in clans)
                {
                    try
                    {
                        _logger.LogInformation($"Processing clan {clan.Name} ({clan.Tag})");

                        // Update clan basic information
                        var updateResult = await UpdateClanAsync(clan.Tag);
                        if (updateResult.Success)
                        {
                            successfulUpdates++;
                            _logger.LogInformation($"Successfully updated clan {clan.Name}");
                        }
                        else
                        {
                            failedUpdates++;
                            _logger.LogWarning($"Failed to update clan {clan.Name}: {updateResult.Message}");
                        }

                        // Update clan history regardless of basic update result
                        var historyResult = await UpdateClanHistoryAsync(clan);
                        if (historyResult.Success)
                        {
                            successfulHistoryUpdates++;
                            _logger.LogInformation($"Successfully updated history for clan {clan.Name}");
                        }
                        else
                        {
                            failedHistoryUpdates++;
                            _logger.LogWarning($"Failed to update history for clan {clan.Name}: {historyResult.Message}");
                        }

                        // Add a small delay between API calls to be respectful to the API
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        failedUpdates++;
                        failedHistoryUpdates++;
                        _logger.LogError(ex, $"Unexpected error processing clan {clan.Name} ({clan.Tag})");
                    }
                }

                var summary = $"Weekly update completed. " +
                             $"Clan Updates: {successfulUpdates} successful, {failedUpdates} failed. " +
                             $"History Updates: {successfulHistoryUpdates} successful, {failedHistoryUpdates} failed.";

                _logger.LogInformation(summary);

                if (failedUpdates == 0 && failedHistoryUpdates == 0)
                {
                    return ServiceResult.Successful(summary);
                }
                else if (successfulUpdates > 0 || successfulHistoryUpdates > 0)
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
                    _logger.LogWarning($"Invalid clan tag provided: {clanTag}");
                    return ServiceResult.Failure(sanitizedTag.errorMessage);
                }

                var tag = sanitizedTag.sanitizedTag;
                _logger.LogInformation($"Adding clan with tag {tag}");
                var clan = await _clashRoyaleService.GetClanByTagAsync(tag);
                if (clan == null)
                {
                    _logger.LogWarning($"Clan with tag {tag} not found in API");
                    return ServiceResult.Failure($"Clan with tag '{tag}' not found in API");
                }

                clan.Tag = Regex.Replace(clan.Tag, @"[^a-zA-Z0-9]", "");

                _logger.LogInformation($"Clan {clan.Name} with tag {clan.Tag} found. Adding to database");
                if (await _clanRepository.AddClanAsync(clan))
                {
                    _logger.LogInformation($"Successfully added {clan.Name} with tag {clan.Tag} to database");
                    return ServiceResult.Successful($"{clan.Name} successfully added to Clans!");
                }
                else
                {
                    _logger.LogWarning($"Clan with tag {clan.Tag} already exists in database");
                    return ServiceResult.Failure($"Clan with tag '{clan.Tag}' already exists in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while adding clan with tag {clanTag}");
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
                    _logger.LogWarning($"Invalid clan tag provided: {clanTag}");
                    return ServiceResult<Clan>.Failure(sanitizedTag.errorMessage);
                }
                var tag = sanitizedTag.sanitizedTag;

                _logger.LogInformation($"Retrieving clan with tag {tag} from database");
                var clan = await _clanRepository.GetClanAsync(tag);
                if (clan == null)
                {
                    _logger.LogWarning($"Clan with tag {tag} not found in database");
                    return ServiceResult<Clan>.Failure($"Clan with tag '{tag}' not found in database");
                }

                _logger.LogInformation($"Successfully retrieved clan {clan.Name} with tag {clan.Tag}");
                return ServiceResult<Clan>.Successful(clan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while retrieving clan with tag {clanTag}");
                return ServiceResult<Clan>.Failure($"An unexpected error occurred while retrieving clan with tag {clanTag}");
            }
        }

        public async Task<ServiceResult> DeleteClanAsync(string clanTag)
        {
            try
            {
                _logger.LogInformation($"Deleting clan with tag {clanTag}");
                if (await _clanRepository.DeleteClanAsync(clanTag))
                {
                    _logger.LogInformation($"Successfully deleted clan with tag {clanTag}");
                    return ServiceResult.Successful($"Clan with tag {clanTag} successfully deleted");
                }
                else
                {
                    _logger.LogWarning($"Clan with tag {clanTag} not found in database");
                    return ServiceResult.Failure($"Clan with tag {clanTag} not found in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while deleting clan with tag {clanTag}");
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
                    _logger.LogWarning($"Invalid clan tag provided: {clanTag}");
                    return ServiceResult.Failure(sanitizedTag.errorMessage);
                }

                var tag = sanitizedTag.sanitizedTag;
                _logger.LogInformation($"Updating clan with tag {tag}");
                var updatedClan = await _clashRoyaleService.GetClanByTagAsync(tag);

                if (updatedClan == null)
                {
                    _logger.LogWarning($"Clan with tag {tag} not found in API");
                    return ServiceResult.Failure($"Clan with tag '{tag}' not found in API");
                }

                updatedClan.Tag = Regex.Replace(updatedClan.Tag, @"[^a-zA-Z0-9]", "");
                _logger.LogInformation($"Clan {updatedClan.Name} with tag {updatedClan.Tag} found. Updating in database");

                if (await _clanRepository.UpdateClanAsync(updatedClan))
                {
                    _logger.LogInformation($"Successfully updated {updatedClan.Name} with tag {updatedClan.Tag} in database");
                    return ServiceResult.Successful($"{updatedClan.Name} successfully updated in Clans!");
                }
                else
                {
                    _logger.LogWarning($"Clan with tag {updatedClan.Tag} does not exist in database");
                    return ServiceResult.Failure($"Clan with tag '{updatedClan.Tag}' does not exist in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while updating clan with tag {clanTag}");
                return ServiceResult.Failure($"An unexpected error occurred while updating clan with tag {clanTag}");
            }
        }

        public async Task<ServiceResult> UpdateClanHistoryAsync(Clan clan)
        {
            try
            {
                _logger.LogInformation($"Updating history for clan {clan.Name}");

                var riverRaceLog = await _clashRoyaleService.GetRiverRaceLogAsync(clan.Tag);
                if (riverRaceLog == null || riverRaceLog.Items == null || !riverRaceLog.Items.Any())
                {
                    _logger.LogWarning($"No war log data found for clan with tag {clan.Tag}");
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
                    _logger.LogWarning($"No valid clan standings found in war log for clan with tag {clan.Tag}");
                    return ServiceResult.Failure($"No valid clan standings found in war log for clan with tag '{clan.Tag}'");
                }

                if (await _clanRepository.UpdateClanHistoryAsync(clan, clanHistories))
                {
                    _logger.LogInformation($"Successfully updated history for {clan.Name} in database");
                    return ServiceResult.Successful($"{clan.Name} history successfully updated!");
                }
                else
                {
                    _logger.LogWarning($"Failed to update history for clan with tag {clan.Tag}");
                    return ServiceResult.Failure($"Failed to update history for clan with tag '{clan.Tag}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while updating history for clan with tag {clan.Tag}");
                return ServiceResult.Failure($"An unexpected error occurred while updating history for clan with tag {clan.Tag}");
            }
        }
    }
}
