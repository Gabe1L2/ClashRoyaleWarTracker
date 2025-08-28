using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ClashRoyaleProject.Infrastructure.Repositories
{
    public class ClanRepository : IClanRepository
    {
        private readonly ApplicationDbContext _context;

        public ClanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddClanAsync(Clan clan)
        {
            try
            {
                var curClan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clan.Tag);

                if (curClan == null)
                {
                    clan.LastUpdated = DateTime.Now;
                    await _context.Clans.AddAsync(clan);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add clan {clan.Tag}", ex);
            }
        }

        public async Task<IEnumerable<Clan>> GetAllClansAsync()
        {
            try
            {
                return await _context.Clans.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve clans from the database", ex);
            }
        }

        public async Task<bool> DeleteClanAsync(string clanTag)
        {
            try
            {
                var delCount = await _context.Clans
                    .Where(c => c.Tag == clanTag)
                    .ExecuteDeleteAsync();

                return delCount > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete clan with tag {clanTag}", ex);
            }
            
        }

        public async Task<bool> UpdateClanAsync(Clan clan)
        {
            try
            {
                var curClan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clan.Tag);
                if (curClan == null)
                {
                    return false;
                }

                curClan.Name = clan.Name;
                curClan.WarTrophies = clan.WarTrophies;
                curClan.LastUpdated = DateTime.Now;

                _context.Clans.Update(curClan);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add/update clan {clan.Tag}", ex);
            }
        }
    }
}
