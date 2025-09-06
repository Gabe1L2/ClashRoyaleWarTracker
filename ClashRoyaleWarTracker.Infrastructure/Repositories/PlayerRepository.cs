using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _context;
        public PlayerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetPlayerAsync(string playerTag)
        {
            try
            {
                var player = await _context.Players.FirstOrDefaultAsync(p => p.Tag == playerTag);
                return player;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve player with tag {playerTag} from the database", ex);
            }
        }

        public async Task<Player?> AddPlayerAsync(Player player)
        {
            try
            {
                var existingPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Tag == player.Tag);
                if (existingPlayer != null)
                {
                    return null; // Player already exists
                }
                await _context.Players.AddAsync(player);
                await _context.SaveChangesAsync();
                return player;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add player {player.Tag}", ex);
            }
        }
    }
}