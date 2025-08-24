using ClashRoyaleProject.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ClashRoyaleProject.Application.Interfaces;

namespace ClashRoyaleProject.Infrastructure.Repositories
{
    public class PlayerRepository : IRepository
    {
        private readonly ApplicationDbContext _context;
        public PlayerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetPlayerByTagAsync(string tag)
        {
            return await _context.Players.FirstOrDefaultAsync(p => p.Tag == tag);
        }

        public async Task<IEnumerable<Player>> GetAllPlayersAsync()
        {
            return await _context.Players.ToListAsync();
        }

        public async Task AddOrUpdatePlayerAsync(Player player)
        {
            var existing = await _context.Players.FirstOrDefaultAsync(p => p.Tag == player.Tag);
            if (existing == null)
            {
                await _context.Players.AddAsync(player);
            }
            else
            {
                existing.Name = player.Name;
                existing.LastUpdated = player.LastUpdated;
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeletePlayerAsync(string tag)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Tag == tag);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
        }
    }
}