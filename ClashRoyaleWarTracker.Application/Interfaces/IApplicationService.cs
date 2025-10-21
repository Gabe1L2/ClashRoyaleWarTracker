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
        Task<ServiceResult> WeeklyUpdateAsync(int numWeeksForPlayerAverages = 4, string? updatedBy = "system");
        Task<ServiceResult> DataUpdateAsync(int numWeeksWarHistory, int numWeeksPlayerAverages = 4, string? updatedBy = null);
        Task<ServiceResult> AddClanAsync(string clanTag);
        Task<ServiceResult<IEnumerable<Clan>>> GetAllClansAsync();
        Task<ServiceResult<Clan>> GetClanAsync(string clanTag);
        Task<ServiceResult> DeleteClanAsync(string clanTag);
        Task<ServiceResult> UpdateClanAsync(string clanTag);
        Task<ServiceResult> PopulateClanHistoryAsync(Clan clan);
        Task<ServiceResult> PopulatePlayerWarHistories(Clan clan, string? updatedBy = null, int numOfRiverRaces = 1);
        Task<ServiceResult> UpdateAllActivePlayerAverages(int numOfWeeksToUse, bool aboveFiveThousandTrophies);
        Task<ServiceResult> UpdatePlayerAverageAsync(int playerId, int numOfWeeksToUse = 4);
        Task<ServiceResult<IEnumerable<PlayerAverageDTO>>> GetAllPlayerAverageDTOsAsync();
        Task<ServiceResult<IEnumerable<GroupedPlayerWarHistoryDTO>>> GetAllGroupedPlayerWarHistoryDTOsAsync(bool is5k = true);
        Task<ServiceResult<Player>> GetPlayerByIdAsync(int playerId);
        Task<ServiceResult> UpdatePlayerStatusAsync(int playerId, string status, string? updatedBy = null);
        Task<ServiceResult<IEnumerable<PlayerWarHistoryExpanded>>> GetPlayerWarHistoriesByPlayerIdAsync(int playerId);
        Task<ServiceResult> UpdatePlayerWarHistoryAsync(int warHistoryId, int fame, int decksUsed, int boatAttacks, string? updatedBy);
        Task<ServiceResult> AddClanClanHistoryPlayerHistoryAsync(string clanTag, int numWeeksWarHistory, string? updatedBy = null);
        Task<ServiceResult<int>> GetPlayerIdFromWarHistoryAsync(int warHistoryId);
        Task<ServiceResult> UpdatePlayerNotesAsync(int playerId, string? notes, string? updatedBy = null);
        Task<ServiceResult<IEnumerable<RosterAssignmentDTO>>> GetAllRosterAssignmentDTOsAsync();
        Task<ServiceResult> UpdateRosterByFameAverageAsync();
        Task<ServiceResult<IEnumerable<PlayerAverage>>> GetAllActivePlayerAveragesAsync(bool is5k);
        Task<ServiceResult> UpdateAllPlayerAveragesAsync(int numOfWeeksToUse, bool aboveFiveThousandTrophies);
        Task<ServiceResult> UpdateRosterInClanStatusAsync();
        Task<ServiceResult> UpdateRosterInClanStatusForClanAsync(int? clanId);
        Task<ServiceResult> BackupCurrentRosterToNewSeasonWeekAsync(int newSeasonId, int newWeekIndex);
        Task<ServiceResult<IEnumerable<(int SeasonId, int WeekIndex)>>> GetAvailableRosterSeasonWeeksAsync();
        Task<ServiceResult<IEnumerable<RosterAssignmentDTO>>> GetRosterAssignmentsBySeasonWeekAsync(int seasonId, int weekIndex);
        Task<ServiceResult> UpdateRosterAssignmentAsync(int rosterAssignmentId, int? assignedClanId, string? updatedBy = null);
    }
}
