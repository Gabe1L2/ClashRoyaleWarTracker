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

        // Change the method signature to remove the generic type parameter T, since T is not defined or needed.
        // Use ServiceResult instead of ServiceResult<T> for AddClanAsync, and update return statements accordingly.

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
                var clan = await _clashRoyaleService.GetClanByTagAsync(tag);

                if (clan == null)
                {
                    _logger.LogWarning($"Clan with tag {tag} not found in API");
                    return ServiceResult.Failure($"Clan with tag '{tag}' not found in API");
                }

                clan.Tag = Regex.Replace(clan.Tag, @"[^a-zA-Z0-9]", "");
                _logger.LogInformation($"Clan {clan.Name} with tag {clan.Tag} found. Updating in database");

                if (await _clanRepository.UpdateClanAsync(clan))
                {
                    _logger.LogInformation($"Successfully updated {clan.Name} with tag {clan.Tag} in database");
                    return ServiceResult.Successful($"{clan.Name} successfully updated in Clans!");
                }
                else
                {
                    _logger.LogWarning($"Clan with tag {clan.Tag} does not exist in database");
                    return ServiceResult.Failure($"Clan with tag '{clan.Tag}' does not exist in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while updating clan with tag {clanTag}");
                return ServiceResult.Failure($"An unexpected error occurred while updating clan with tag {clanTag}");
            }
        }
    }
}
