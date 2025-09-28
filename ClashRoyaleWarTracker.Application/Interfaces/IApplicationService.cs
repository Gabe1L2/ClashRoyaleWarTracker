using ClashRoyaleWarTracker.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IApplicationService
    {
        Task<ServiceResult> WeeklyUpdateAsync(int numWeeksForPlayerAverages = 4);
        Task<ServiceResult> DataUpdateAsync(int numWeeksWarHistory, int numWeeksPlayerAverages = 4);
        Task<ServiceResult> AddClanAsync(string clanTag);
        Task<ServiceResult<IEnumerable<Clan>>> GetAllClansAsync();
        Task<ServiceResult<Clan>> GetClanAsync(string clanTag);
        Task<ServiceResult> DeleteClanAsync(string clanTag);
        Task<ServiceResult> UpdateClanAsync(string clanTag);
        Task<ServiceResult> PopulateClanHistoryAsync(Clan clan);
        Task<ServiceResult> PopulatePlayerWarHistories(Clan clan, int numOfRiverRaces);
        Task<ServiceResult> UpdateAllActivePlayerAverages(int numOfWeeksToUse, bool aboveFiveThousandTrophies);
        Task<ServiceResult> UpdatePlayerAverageAsync(int playerId, int numOfWeeksToUse = 4);
        Task<ServiceResult<IEnumerable<PlayerAverageDTO>>> GetAllPlayerAverageDTOsAsync();
        Task<ServiceResult<IEnumerable<GroupedPlayerWarHistoryDTO>>> GetAllGroupedPlayerWarHistoryDTOsAsync(bool is5k = true);
        Task<ServiceResult<Player>> GetPlayerByIdAsync(int playerId);
        Task<ServiceResult> UpdatePlayerStatusAsync(int playerId, string status);
        Task<ServiceResult<IEnumerable<PlayerWarHistoryExpanded>>> GetPlayerWarHistoriesByPlayerIdAsync(int playerId);
        Task<ServiceResult> UpdatePlayerWarHistoryAsync(int warHistoryId, int fame, int decksUsed, int boatAttacks);
        Task<ServiceResult> AddClanClanHistoryPlayerHistoryAsync(string clanTag, int numWeeksWarHistory);
        Task<ServiceResult<int>> GetPlayerIdFromWarHistoryAsync(int warHistoryId);
        Task<ServiceResult> UpdatePlayerNotesAsync(int playerId, string? notes);
        Task<ServiceResult<IEnumerable<RosterAssignmentDTO>>> GetAllRosterAssignmentDTOsAsync();
        Task<ServiceResult> UpdateRosterByFameAverageAsync();
        Task<ServiceResult<IEnumerable<PlayerAverage>>> GetAllActivePlayerAveragesAsync(bool is5k);
        Task<ServiceResult> UpdateAllPlayerAveragesAsync(int numOfWeeksToUse, bool aboveFiveThousandTrophies);
        Task<ServiceResult> UpdateRosterInClanStatusAsync();
        Task<ServiceResult> UpdateRosterInClanStatusForClanAsync(int? clanId);
    }
}
