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
        Task<ServiceResult> WeeklyUpdateAsync();
        Task<ServiceResult> AddClanAsync(string clanTag);
        Task<ServiceResult<IEnumerable<Clan>>> GetAllClansAsync();
        Task<ServiceResult<Clan>> GetClanAsync(string clanTag);
        Task<ServiceResult> DeleteClanAsync(string clanTag);
        Task<ServiceResult> UpdateClanAsync(string clanTag);
        Task<ServiceResult> PopulateClanHistoryAsync(Clan clan);
        Task<ServiceResult> PopulatePlayerWarHistories(Clan clan, int numOfRiverRaces);
        Task<ServiceResult> UpdateAllActivePlayerAverages(int numOfWeeksToUse, bool aboveFiveThousandTrophies);
        Task<ServiceResult<IEnumerable<PlayerAverageDTO>>> GetAllPlayerAveragesAsync();
        //Task<ServiceResult> UpdateAllActivePlayerClanAssociations();
    }
}
