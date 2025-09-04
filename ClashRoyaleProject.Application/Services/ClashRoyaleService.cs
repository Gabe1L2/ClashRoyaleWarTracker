using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashRoyaleProject.Application.Services
{
    public class ClashRoyaleService : IClashRoyaleService
    {
        private readonly IClashRoyaleApiClient _apiClient;
        private readonly ILogger<ClashRoyaleService> _logger;

        public ClashRoyaleService(IClashRoyaleApiClient apiClient, ILogger<ClashRoyaleService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<Clan?> GetClanByTagAsync(string clanTag)
        {
            try
            {
                var clan = await _apiClient.GetClanByTagAsync(clanTag);
                return clan; // this will be null if not found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching clan with tag {clanTag}");
                throw;
            }
        }

        public async Task<RiverRaceLogResponse?> GetRiverRaceLogAsync(string clanTag)
        {
            try
            {
                var rawWarLog = await _apiClient.GetRiverRaceLogAsync(clanTag);
                return rawWarLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching war log for clan with tag {clanTag}");
                throw;
            }
        }
    }
}
