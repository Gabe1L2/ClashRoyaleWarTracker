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

        public async Task AddOrUpdateClanAsync(Clan clan)
        {
            try
            {
                var curClan = await _context.Clans.FirstOrDefaultAsync(c => c.Tag == clan.Tag);

                if (curClan == null)
                {
                    clan.Tag = Regex.Replace(clan.Tag,"#","");
                    clan.LastUpdated = DateTime.Now;
                    await _context.Clans.AddAsync(clan);
                }
                else
                {
                    curClan.Tag = Regex.Replace(clan.Tag, "#", "");
                    curClan.Name = clan.Name;
                    curClan.WarTrophies = clan.WarTrophies;
                    curClan.LastUpdated = DateTime.Now;
                    _context.Clans.Update(curClan);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add/update clan {clan.Tag}", ex);
            }
        }

    }
}
