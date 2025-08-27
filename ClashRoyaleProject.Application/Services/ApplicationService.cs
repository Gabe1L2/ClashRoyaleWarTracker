using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public async Task<ServiceResult> AddClanAsync(string clanTag)
        {
            try
            {
                // Input Validation
                var sanitizedTag = ValidateAndSanitizeClanTag(clanTag);
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

                _logger.LogInformation($"Clan {clan.Name} with tag {tag} found. Adding to database");
                await _clanRepository.AddOrUpdateClanAsync(clan);

                _logger.LogInformation($"Successfully added {clan.Name} with tag {tag} to database");
                return ServiceResult.Successful($"{clan.Name} successfully added to Clans!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while adding clan with tag {clanTag}");
                return ServiceResult.Failure($"An unexpected error occurred while adding clan with tag {clanTag}");
            }
        }

        private static (bool isValid, string sanitizedTag, string errorMessage) ValidateAndSanitizeClanTag(string clanTag)
        {
            if (string.IsNullOrWhiteSpace(clanTag))
            {
                return (false, string.Empty, "Clan tag cannot be empty");
            }

            // Remove all non-alphanumeric characters (including spaces, special chars, etc.)
            var sanitized = Regex.Replace(clanTag.Trim(), @"[^a-zA-Z0-9]", "");

            if (string.IsNullOrEmpty(sanitized))
            {
                return (false, string.Empty, "Clan tag must contain at least one letter or number");
            }

            if (sanitized.Length > 25)
            {
                return (false, string.Empty, "Clan tag cannot exceed 25 characters");
            }

            // Clash Royale clan tags are typically 8-9 characters, but let's be flexible
            // Most real clan tags are between 3-15 characters after removing the #
            if (sanitized.Length < 3)
            {
                return (false, string.Empty, "Clan tag must be at least 3 characters long");
            }

            // Add # prefix if not already present (Clash Royale format)

            return (true, sanitized, string.Empty);
        }
    }
}
