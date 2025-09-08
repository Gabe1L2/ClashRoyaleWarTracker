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
        public PlayerRepository(ApplicationDbContext context, ILogger<PlayerRepository> logger)
        {
            _context = context;
            _logger = logger;
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
    }
}