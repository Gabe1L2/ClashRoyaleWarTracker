using ClashRoyaleWarTracker.Application.Models;
using System.ComponentModel;
using System.Numerics;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerAsync(string playerTag);
        Task<Player?> GetPlayerByIdAsync(int playerId);
        Task<int> AddPlayerAsync(Player player, string? updatedBy = null);
        Task<List<Player>> GetAllActivePlayersAsync();
        Task<List<Player>> GetL2WPlayersAsync();
        Task<bool> UpdatePlayerStatusAsync(int playerId, string status, string? updatedBy = null);
        Task UpsertPlayerAverageAsync(PlayerAverage playerAverage);
        Task<List<PlayerAverageDTO>> GetAllPlayerAverageDTOsAsync();
        Task<bool> UpdatePlayerNotesAsync(int playerId, string? notes, string? updatedBy = null);
        Task<IEnumerable<RosterAssignmentDTO>> GetAllRosterAssignmentDTOsAsync();
        Task<IEnumerable<PlayerAverage>> GetAllActivePlayerAveragesAsync(bool is5k);
        Task<List<Player>> GetAllPlayersAsync();
        Task<bool> BulkUpsertRosterAssignmentsAsync(List<RosterAssignment> rosterAssignments, string? updatedBy = null);
        Task<bool> UpdateRosterAssignmentInClanStatusAsync(int rosterAssignmentId, bool isInClan);
        Task<List<RosterAssignmentDTO>> GetRosterAssignmentsForOneWeekOneClanAsync(int seasonId, int weekIndex, int? clanId);
        Task<bool> CopyRosterAssignmentsToNewSeasonWeekAsync(int currentSeasonId, int currentWeekIndex, int newSeasonId, int newWeekIndex);
        Task<IEnumerable<(int SeasonId, int WeekIndex)>> GetDistinctRosterSeasonWeeksAsync();
        Task<IEnumerable<RosterAssignmentDTO>> GetRosterAssignmentsBySeasonWeekAsync(int seasonId, int weekIndex);
        Task<bool> UpdateRosterAssignmentClanAsync(int rosterAssignmentId, int? clanId, string? updatedBy = null);
    }
}
