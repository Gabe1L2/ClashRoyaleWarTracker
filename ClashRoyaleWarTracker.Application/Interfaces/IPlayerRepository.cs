using ClashRoyaleWarTracker.Application.Models;
using System.ComponentModel;
using System.Numerics;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerAsync(string playerTag);
        Task<Player?> GetPlayerByIdAsync(int playerId);
        Task<int> AddPlayerAsync(Player player);
        Task<List<Player>> GetAllActivePlayersAsync();
        Task<bool> UpdatePlayerStatusAsync(int playerId, string status);
        Task UpsertPlayerAverageAsync(PlayerAverage playerAverage);
        Task<List<PlayerAverageDTO>> GetAllPlayerAverageDTOsAsync();
        Task<bool> UpdatePlayerNotesAsync(int playerId, string? notes);
        Task<IEnumerable<RosterAssignmentDTO>> GetAllRosterAssignmentDTOsAsync();
        Task<IEnumerable<PlayerAverage>> GetAllActivePlayerAveragesAsync(bool is5k);
        Task<List<Player>> GetAllPlayersAsync();
        Task<bool> BulkUpsertRosterAssignmentsAsync(List<RosterAssignment> rosterAssignments);
        Task<bool> UpdateRosterAssignmentInClanStatusAsync(int rosterAssignmentId, bool isInClan);
        Task<List<RosterAssignmentDTO>> GetRosterAssignmentsForOneWeekOneClanAsync(int seasonId, int weekIndex, int? clanId);
    }
}
