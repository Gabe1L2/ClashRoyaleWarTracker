using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlayerRepository> _logger;
        private readonly ITimeZoneService _timeZoneService;
        public PlayerRepository(ApplicationDbContext context, ILogger<PlayerRepository> logger, ITimeZoneService timeZoneService)
        {
            _context = context;
            _logger = logger;
            _timeZoneService = timeZoneService;
        }

        public async Task<Player?> GetPlayerAsync(string playerTag)
        {
            try
            {
                _logger.LogDebug("Retrieving player with tag {PlayerTag} from database", playerTag);
                var player = await _context.Players.FirstOrDefaultAsync(p => p.Tag == playerTag);
                if (player != null)
                {
                    _logger.LogDebug("Found player {PlayerName} with tag {PlayerTag}", player.Name, playerTag);
                }
                else
                {
                    _logger.LogDebug("No player found with tag {PlayerTag}", playerTag);
                }
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve player with tag {PlayerTag} from the database", playerTag);
                throw new InvalidOperationException($"Failed to retrieve player with tag {playerTag} from the database", ex);
            }
        }

        public async Task<int> AddPlayerAsync(Player player)
        {
            try
            {
                _logger.LogDebug("Adding new player {PlayerName} with tag {PlayerTag} to database", player.Name, player.Tag);
                player.LastUpdated = _timeZoneService.Now;
                await _context.Players.AddAsync(player);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully added player {PlayerName} with tag {PlayerTag}", player.Name, player.Tag);
                return player.ID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add player {PlayerTag} to the database", player.Tag);
                throw new InvalidOperationException($"Failed to add player {player.Tag}", ex);
            }
        }

        public async Task<List<Player>> GetAllActivePlayersAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all active players from database");

                var players = await _context.Players
                    .Where(p => p.Status == "Active")
                    .ToListAsync();

                _logger.LogDebug("Found {Count} active players", players.Count);
                return players;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active players from the database");
                throw new InvalidOperationException("Failed to retrieve active players from the database", ex);
            }
        }

        public async Task UpsertPlayerAverageAsync(PlayerAverage newPlayerAverage)
        {
            try
            {
                _logger.LogDebug("Upserting player average for PlayerID {PlayerId}, Is5k: {Is5k}", newPlayerAverage.PlayerID, newPlayerAverage.Is5k);

                var existingAverage = await _context.PlayerAverages
                    .FirstOrDefaultAsync(pa => pa.PlayerID == newPlayerAverage.PlayerID && pa.Is5k == newPlayerAverage.Is5k);

                if (existingAverage != null)
                {
                    // Update existing record
                    existingAverage.ClanID = newPlayerAverage.ClanID;
                    existingAverage.FameAttackAverage = newPlayerAverage.FameAttackAverage;
                    existingAverage.Attacks = newPlayerAverage.Attacks;
                    existingAverage.LastUpdated = _timeZoneService.Now;
                    _context.PlayerAverages.Update(existingAverage);
                    _logger.LogDebug("Updated existing player average for PlayerID {PlayerId}", newPlayerAverage.PlayerID);
                }
                else
                {
                    newPlayerAverage.LastUpdated = _timeZoneService.Now;
                    await _context.PlayerAverages.AddAsync(newPlayerAverage);
                    _logger.LogDebug("Added new player average for PlayerID {PlayerId}", newPlayerAverage.PlayerID);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert player average for PlayerID {PlayerId}", newPlayerAverage.PlayerID);
                throw new InvalidOperationException($"Failed to upsert player average for PlayerID {newPlayerAverage.PlayerID}", ex);
            }
        }

        public async Task<List<PlayerAverageDTO>> GetAllPlayerAveragesAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all player averages with player and clan details");

                var playerAverages = await _context.PlayerAverages
                    .Join(_context.Players, pa => pa.PlayerID, p => p.ID, (pa, p) => new { pa, p })
                    .GroupJoin(_context.Clans, x => x.pa.ClanID, c => c.ID, (x, clans) => new { x.pa, x.p, clans })
                    .SelectMany(x => x.clans.DefaultIfEmpty(), (x, c) => new PlayerAverageDTO
                    {
                        ID = x.pa.ID,
                        PlayerID = x.pa.PlayerID,
                        PlayerName = x.p.Name ?? "Unknown",
                        PlayerTag = x.p.Tag,
                        ClanID = x.pa.ClanID,
                        ClanName = c != null ? c.Name : "No Clan",
                        FameAttackAverage = x.pa.FameAttackAverage,
                        Attacks = x.pa.Attacks,
                        Is5k = x.pa.Is5k,
                        LastUpdated = x.pa.LastUpdated,
                        Status = x.p.Status
                    })
                    .OrderByDescending(dto => dto.FameAttackAverage)
                    .ToListAsync();

                _logger.LogDebug("Found {Count} player averages", playerAverages.Count);
                return playerAverages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve player averages from the database");
                throw new InvalidOperationException("Failed to retrieve player averages from the database", ex);
            }
        }

        public async Task<Player?> GetPlayerByIdAsync(int playerId)
        {
            try
            {
                _logger.LogDebug("Retrieving player with ID {PlayerId} from database", playerId);
                var player = await _context.Players.FirstOrDefaultAsync(p => p.ID == playerId);
                if (player != null)
                {
                    _logger.LogDebug("Found player {PlayerName} with ID {PlayerId}", player.Name, playerId);
                }
                else
                {
                    _logger.LogDebug("No player found with ID {PlayerId}", playerId);
                }
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve player with ID {PlayerId} from the database", playerId);
                throw new InvalidOperationException($"Failed to retrieve player with ID {playerId} from the database", ex);
            }
        }

        public async Task<bool> UpdatePlayerStatusAsync(int playerId, string status)
        {
            try
            {
                _logger.LogDebug("Updating player status for PlayerID {PlayerId} to {Status}", playerId, status);

                var player = await _context.Players.FirstOrDefaultAsync(p => p.ID == playerId);
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found", playerId);
                    return false;
                }

                player.Status = status;
                player.LastUpdated = _timeZoneService.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated player {PlayerName} status to {Status}", player.Name, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update player status for PlayerID {PlayerId}", playerId);
                throw new InvalidOperationException($"Failed to update player status for PlayerID {playerId}", ex);
            }
        }
    }
}