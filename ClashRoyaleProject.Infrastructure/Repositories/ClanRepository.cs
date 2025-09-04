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

        public async Task<Clan?> GetClanAsync(string clanTag)
        {
            try
            {
                return await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clanTag);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve clan with tag {clanTag} from the database", ex);
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

        public async Task<bool> UpdateClanHistoryAsync(Clan clan, List<ClanHistory> clanHistories)
        {
            try
            {
                var curClan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clan.Tag);
                if (curClan == null)
                {
                    return false;
                }

                foreach (var clanHistory in clanHistories)
                {
                    var existingHistory = await _context.ClanHistories.FirstOrDefaultAsync(ch =>
                        ch.ClanID == curClan.ID &&
                        ch.SeasonID == clanHistory.SeasonID &&
                        ch.WeekIndex == clanHistory.WeekIndex);

                    if (existingHistory == null)
                    {
                        clanHistory.ClanID = curClan.ID;
                        await _context.ClanHistories.AddAsync(clanHistory);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update clan history for {clan.Tag}", ex);
            }
        }
    }
}
