using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleWarTracker.Web.Pages
{
    [Authorize]
    public class WarHistoriesModel : BasePageModel
    {
        private readonly IApplicationService _applicationService;
        private readonly ILogger<WarHistoriesModel> _logger;

        public WarHistoriesModel(IApplicationService applicationService, ILogger<WarHistoriesModel> logger, IUserRoleService userRoleService) : base(userRoleService)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        public List<GroupedPlayerWarHistoryDTO> GroupedPlayerWarHistories { get; set; } = new();
        public List<PlayerSpreadsheetRow> PlayerRows { get; set; } = new();
        public List<string> SeasonWeekHeaders { get; set; } = new();
        public List<string> AllClans { get; set; } = new();
        public int TotalRecords { get; set; }

        // ADD: Dictionary to store player averages
        public Dictionary<int, PlayerAverageDTO> PlayerAverages { get; set; } = new();

        [BindProperty]
        public bool Is5kTrophies { get; set; } = true;

        public async Task<IActionResult> OnGetAsync(bool is5k = true)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanViewWarHistory)
                {
                    _logger.LogWarning("User {UserName} attempted to access War History without proper permissions", User.Identity?.Name);
                    return Forbid();
                } 

                Is5kTrophies = is5k;

                // Load grouped war histories
                var result = await _applicationService.GetAllGroupedPlayerWarHistoryDTOsAsync(Is5kTrophies);
                if (result.Success && result.Data != null)
                {
                    GroupedPlayerWarHistories = result.Data.ToList();
                    TotalRecords = GroupedPlayerWarHistories.Count;
                    
                    // ADD: Load player averages
                    await LoadPlayerAveragesAsync();
                    
                    // Transform data into spreadsheet format
                    CreateSpreadsheetData();
                    
                    _logger.LogInformation("Successfully loaded {Count} grouped war history records for {TrophyLevel}", 
                        TotalRecords, Is5kTrophies ? "5k+" : "sub-5k");
                }
                else
                {
                    _logger.LogWarning("Failed to load grouped player war histories: {Message}", result.Message);
                    GroupedPlayerWarHistories = new List<GroupedPlayerWarHistoryDTO>();
                    PlayerRows = new List<PlayerSpreadsheetRow>();
                    SeasonWeekHeaders = new List<string>();
                    AllClans = new List<string>();
                    PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
                    TotalRecords = 0;
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading war histories data");
                GroupedPlayerWarHistories = new List<GroupedPlayerWarHistoryDTO>();
                PlayerRows = new List<PlayerSpreadsheetRow>();
                SeasonWeekHeaders = new List<string>();
                AllClans = new List<string>();
                PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
                TotalRecords = 0;
                TempData["ErrorMessage"] = "An error occurred while loading the war histories.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostToggleTrophyLevelAsync()
        {
            return RedirectToPage("WarHistories", new { is5k = Is5kTrophies });
        }

        // ADD: New method to load player averages
        private async Task LoadPlayerAveragesAsync()
        {
            try
            {
                var averagesResult = await _applicationService.GetAllPlayerAveragesAsync();
                if (averagesResult.Success && averagesResult.Data != null)
                {
                    // Filter by trophy level and create dictionary for quick lookup
                    PlayerAverages = averagesResult.Data
                        .Where(pa => pa.Is5k == Is5kTrophies)
                        .ToDictionary(pa => pa.PlayerID, pa => pa);
                    
                    _logger.LogInformation("Loaded {Count} player averages for {TrophyLevel}", PlayerAverages.Count, Is5kTrophies ? "5k+" : "sub-5k");
                }
                else
                {
                    _logger.LogWarning("Failed to load player averages: {Message}", averagesResult.Message);
                    PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading player averages");
                PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
            }
        }

        private void CreateSpreadsheetData()
        {
            // Get all unique season/week combinations and sort them
            var allSeasonWeeks = GroupedPlayerWarHistories
                .Select(g => new { g.SeasonID, g.WeekIndex })
                .Distinct()
                .OrderByDescending(sw => sw.SeasonID)
                .ThenByDescending(sw => sw.WeekIndex)
                .ToList();

            SeasonWeekHeaders = allSeasonWeeks
                .Select(sw => $"{sw.SeasonID}-{sw.WeekIndex}")
                .ToList();

            // Get all unique clans for filtering
            AllClans = GroupedPlayerWarHistories
                .Select(g => g.ClanName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            // Group by player and create rows
            var playerGroups = GroupedPlayerWarHistories
                .GroupBy(g => new { g.PlayerID, g.PlayerTag, g.PlayerName, g.IsActive })
                .OrderBy(pg => pg.Key.PlayerName);

            PlayerRows = new List<PlayerSpreadsheetRow>();

            foreach (var playerGroup in playerGroups)
            {
                var row = new PlayerSpreadsheetRow
                {
                    PlayerID = playerGroup.Key.PlayerID,
                    PlayerTag = playerGroup.Key.PlayerTag,
                    PlayerName = playerGroup.Key.PlayerName,
                    IsActive = playerGroup.Key.IsActive,
                    ClanName = playerGroup.OrderByDescending(g => g.LastUpdated).First().ClanName,
                    WarData = new Dictionary<string, PlayerWarDataCell>()
                };

                // Populate war data for each season/week
                foreach (var seasonWeek in allSeasonWeeks)
                {
                    var key = $"{seasonWeek.SeasonID}-{seasonWeek.WeekIndex}";
                    var warRecord = playerGroup.FirstOrDefault(g => 
                        g.SeasonID == seasonWeek.SeasonID && g.WeekIndex == seasonWeek.WeekIndex);

                    if (warRecord != null)
                    {
                        row.WarData[key] = new PlayerWarDataCell
                        {
                            Fame = warRecord.Fame,
                            DecksUsed = warRecord.DecksUsed,
                            HasData = true
                        };
                    }
                    else
                    {
                        row.WarData[key] = new PlayerWarDataCell
                        {
                            Fame = 0,
                            DecksUsed = 0,
                            HasData = false
                        };
                    }
                }

                PlayerRows.Add(row);
            }
        }
    }

    public class PlayerSpreadsheetRow
    {
        public int PlayerID { get; set; }
        public string PlayerTag { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ClanName { get; set; } = string.Empty;
        public Dictionary<string, PlayerWarDataCell> WarData { get; set; } = new();
    }

    public class PlayerWarDataCell
    {
        public int Fame { get; set; }
        public int DecksUsed { get; set; }
        public bool HasData { get; set; }
        public string DisplayText => HasData ? $"{Fame}/{DecksUsed}" : "-";
    }
}
