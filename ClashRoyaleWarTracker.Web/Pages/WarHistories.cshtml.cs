//using ClashRoyaleWarTracker.Application.Interfaces;
//using ClashRoyaleWarTracker.Application.Models;
//using ClashRoyaleWarTracker.Web.Pages.Shared;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace ClashRoyaleWarTracker.Web.Pages
//{
//    [Authorize]
//    public class WarHistoriesModel : BasePageModel
//    {
//        private readonly IApplicationService _applicationService;
//        private readonly ILogger<WarHistoriesModel> _logger;

//        public WarHistoriesModel(IApplicationService applicationService, ILogger<WarHistoriesModel> logger, IUserRoleService userRoleService) : base(userRoleService)
//        {
//            _applicationService = applicationService;
//            _logger = logger;
//        }

//        public List<GroupedPlayerWarHistoryDTO> GroupedPlayerWarHistories { get; set; } = new();
//        public List<PlayerSpreadsheetRow> PlayerRows { get; set; } = new();
//        public List<string> SeasonWeekHeaders { get; set; } = new();
//        public IList<Clan> AllClans { get; set; } = new List<Clan>();
//        public List<string> AllStatuses { get; set; } = new();
//        public int TotalRecords { get; set; }

//        // ADD: Dictionary to store player averages
//        public Dictionary<int, PlayerAverageDTO> PlayerAverages { get; set; } = new();

//        [BindProperty]
//        public bool Is5kTrophies { get; set; } = true;
//        [BindProperty]
//        public string ClanTag { get; set; } = string.Empty;

//        public async Task<IActionResult> OnGetAsync(bool is5k = true)
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();
//                if (!CanViewWarHistory)
//                {
//                    _logger.LogWarning("User {UserName} attempted to access War History without proper permissions", User.Identity?.Name);
//                    return Forbid();
//                } 

//                Is5kTrophies = is5k;

//                // Load grouped war histories
//                var result = await _applicationService.GetAllGroupedPlayerWarHistoryDTOsAsync(Is5kTrophies);
//                if (result.Success && result.Data != null)
//                {
//                    GroupedPlayerWarHistories = result.Data.ToList();
//                    TotalRecords = GroupedPlayerWarHistories.Count;
                    
//                    // ADD: Load player averages
//                    await LoadPlayerAveragesAsync();
                    
//                    // Transform data into spreadsheet format
//                    CreateSpreadsheetData();
                    
//                    _logger.LogInformation("Successfully loaded {Count} grouped war history records for {TrophyLevel}", 
//                        TotalRecords, Is5kTrophies ? "5k+" : "sub-5k");
//                }
//                else
//                {
//                    _logger.LogWarning("Failed to load grouped player war histories: {Message}", result.Message);
//                    GroupedPlayerWarHistories = new List<GroupedPlayerWarHistoryDTO>();
//                    PlayerRows = new List<PlayerSpreadsheetRow>();
//                    SeasonWeekHeaders = new List<string>();
//                    AllClans = new List<Clan>();
//                    PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
//                    TotalRecords = 0;
//                }

//                // Load all clans for filtering
//                var clansResult = await _applicationService.GetAllClansAsync();
//                if (clansResult.Success && clansResult.Data != null)
//                {
//                    AllClans = clansResult.Data.ToList();
//                }
//                else
//                {
//                    _logger.LogWarning("Failed to load clans: {Message}", clansResult.Message);
//                    AllClans = new List<Clan>();
//                }

//                return Page();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error loading war histories data");
//                GroupedPlayerWarHistories = new List<GroupedPlayerWarHistoryDTO>();
//                PlayerRows = new List<PlayerSpreadsheetRow>();
//                SeasonWeekHeaders = new List<string>();
//                AllClans = new List<Clan>();
//                PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
//                TotalRecords = 0;
//                TempData["ErrorMessage"] = "An error occurred while loading the war histories.";
//                return Page();
//            }
//        }

//        public async Task<IActionResult> OnPostToggleTrophyLevelAsync()
//        {
//            return RedirectToPage("WarHistories", new { is5k = Is5kTrophies });
//        }

//        // ADD: New method to load player averages
//        private async Task LoadPlayerAveragesAsync()
//        {
//            try
//            {
//                var averagesResult = await _applicationService.GetAllPlayerAveragesAsync();
//                if (averagesResult.Success && averagesResult.Data != null)
//                {
//                    // Filter by trophy level and create dictionary for quick lookup
//                    PlayerAverages = averagesResult.Data
//                        .Where(pa => pa.Is5k == Is5kTrophies)
//                        .ToDictionary(pa => pa.PlayerID, pa => pa);
                    
//                    _logger.LogInformation("Loaded {Count} player averages for {TrophyLevel}", PlayerAverages.Count, Is5kTrophies ? "5k+" : "sub-5k");
//                }
//                else
//                {
//                    _logger.LogWarning("Failed to load player averages: {Message}", averagesResult.Message);
//                    PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error loading player averages");
//                PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
//            }
//        }

//        private void CreateSpreadsheetData()
//        {
//            // Get all unique season/week combinations and sort them
//            var allSeasonWeeks = GroupedPlayerWarHistories
//                .Select(g => new { g.SeasonID, g.WeekIndex })
//                .Distinct()
//                .OrderByDescending(sw => sw.SeasonID)
//                .ThenByDescending(sw => sw.WeekIndex)
//                .ToList();

//            SeasonWeekHeaders = allSeasonWeeks
//                .Select(sw => $"{sw.SeasonID}-{sw.WeekIndex}")
//                .ToList();

//            AllStatuses = GroupedPlayerWarHistories
//                .Select(g => g.Status)
//                .Where(status => !string.IsNullOrEmpty(status))
//                .Distinct()
//                .OrderBy(status => status)
//                .ToList();

//            var playerGroups = GroupedPlayerWarHistories
//                .GroupBy(g => new { g.PlayerID, g.PlayerTag, g.PlayerName, g.Status })
//                .OrderBy(pg => pg.Key.PlayerName);

//            PlayerRows = new List<PlayerSpreadsheetRow>();

//            foreach (var playerGroup in playerGroups)
//            {
//                var row = new PlayerSpreadsheetRow
//                {
//                    PlayerID = playerGroup.Key.PlayerID,
//                    PlayerTag = playerGroup.Key.PlayerTag,
//                    PlayerName = playerGroup.Key.PlayerName,
//                    Status = playerGroup.Key.Status,
//                    ClanName = playerGroup.OrderByDescending(g => g.LastUpdated).First().ClanName,
//                    WarData = new Dictionary<string, PlayerWarDataCell>()
//                };

//                // Populate war data for each season/week
//                foreach (var seasonWeek in allSeasonWeeks)
//                {
//                    var key = $"{seasonWeek.SeasonID}-{seasonWeek.WeekIndex}";
//                    var warRecord = playerGroup.FirstOrDefault(g => 
//                        g.SeasonID == seasonWeek.SeasonID && g.WeekIndex == seasonWeek.WeekIndex);

//                    if (warRecord != null)
//                    {
//                        row.WarData[key] = new PlayerWarDataCell
//                        {
//                            Fame = warRecord.Fame,
//                            DecksUsed = warRecord.DecksUsed,
//                            HasData = true
//                        };
//                    }
//                    else
//                    {
//                        row.WarData[key] = new PlayerWarDataCell
//                        {
//                            Fame = 0,
//                            DecksUsed = 0,
//                            HasData = false
//                        };
//                    }
//                }

//                PlayerRows.Add(row);
//            }

//            // ADDED: Sort PlayerRows by Fame Attack Average descending (default sort)
//            PlayerRows = PlayerRows
//                .OrderByDescending(row => {
//                    // Get the fame attack average for this player
//                    if (PlayerAverages.ContainsKey(row.PlayerID))
//                    {
//                        return PlayerAverages[row.PlayerID].FameAttackAverage;
//                    }
//                    return 0; // Default to 0 if no average found
//                })
//                .ThenBy(row => row.PlayerName) // Secondary sort by name for consistency
//                .ToList();
//        }

//        public async Task<IActionResult> OnPostUpdatePlayerStatusAsync(int playerId, string status)
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();
//                if (!CanModifyPlayerData)
//                {
//                    _logger.LogWarning("User {UserName} attempted to update player status without proper permissions", User.Identity?.Name);
//                    return Forbid();
//                }

//                var result = await _applicationService.UpdatePlayerStatusAsync(playerId, status);
//                if (result.Success)
//                {
//                    TempData["SuccessMessage"] = result.Message;
//                }
//                else
//                {
//                    TempData["ErrorMessage"] = result.Message;
//                }

//                return RedirectToPage("WarHistories", new { is5k = Is5kTrophies });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating player status");
//                TempData["ErrorMessage"] = "An error occurred while updating player status.";
//                return RedirectToPage("WarHistories", new { is5k = Is5kTrophies });
//            }
//        }

//        public async Task<JsonResult> OnGetPlayerWarHistoriesAsync(int playerId)
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();
//                if (!CanViewWarHistory)
//                {
//                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
//                }

//                var result = await _applicationService.GetPlayerWarHistoriesByPlayerIdAsync(playerId);
//                if (result.Success)
//                {
//                    return new JsonResult(new { success = true, data = result.Data });
//                }
//                else
//                {
//                    return new JsonResult(new { success = false, message = result.Message });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving player war histories");
//                return new JsonResult(new { success = false, message = "An error occurred while retrieving war histories." });
//            }
//        }

//        public async Task<JsonResult> OnPostUpdateWarHistoryAsync(int warHistoryId, int fame, int decksUsed, int boatAttacks)
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();
//                if (!CanModifyPlayerData)
//                {
//                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
//                }

//                var result = await _applicationService.UpdatePlayerWarHistoryAsync(warHistoryId, fame, decksUsed, boatAttacks);
//                return new JsonResult(new { success = result.Success, message = result.Message });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating war history");
//                return new JsonResult(new { success = false, message = "An error occurred while updating war history." });
//            }
//        }

//        public async Task<IActionResult> OnPostWeeklyUpdateAsync()
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();

//                if (!CanUpdateWarData)
//                {
//                    TempData["ErrorMessage"] = "You don't have permission to update data.";
//                    return RedirectToPage();
//                }

//                var result = await _applicationService.DataUpdateAsync(1);
//                if (result.Success)
//                {
//                    _logger.LogInformation("Weekly update successful: {Message}", result.Message);
//                    TempData["SuccessMessage"] = result.Message;
//                }
//                else
//                {
//                    _logger.LogWarning("Weekly update failed: {Message}", result.Message);
//                    TempData["ErrorMessage"] = result.Message;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during weekly update");
//                TempData["ErrorMessage"] = "An unexpected error occurred during the weekly update.";
//            }

//            return RedirectToPage();
//        }

//        public async Task<IActionResult> OnPostBacklogUpdateAsync()
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();

//                if (!CanUpdateWarData)
//                {
//                    TempData["ErrorMessage"] = "You don't have permission to update data.";
//                    return RedirectToPage();
//                }

//                var result = await _applicationService.DataUpdateAsync(10);
//                if (result.Success)
//                {
//                    TempData["SuccessMessage"] = result.Message;
//                }
//                else
//                {
//                    TempData["ErrorMessage"] = result.Message;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during backlog update");
//                TempData["ErrorMessage"] = "An unexpected error occurred during the backlog update.";
//            }

//            return RedirectToPage();
//        }

//        public async Task<IActionResult> OnPostAddClanAsync()
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();

//                if (!CanManageClans)
//                {
//                    TempData["ErrorMessage"] = "You don't have permission to manage clans.";
//                    return RedirectToPage();
//                }

//                var result = await _applicationService.AddClanAsync(ClanTag ?? string.Empty);
//                if (result.Success)
//                {
//                    TempData["SuccessMessage"] = result.Message;
//                }
//                else
//                {
//                    TempData["ErrorMessage"] = result.Message;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error adding clan with tag: {ClanTag}", ClanTag);
//                TempData["ErrorMessage"] = "An unexpected error occurred while adding the clan. Please try again.";
//            }

//            return RedirectToPage();
//        }

//        public async Task<IActionResult> OnPostDeleteClanAsync()
//        {
//            try
//            {
//                await LoadUserPermissionsAsync();

//                if (!CanManageClans)
//                {
//                    TempData["ErrorMessage"] = "You don't have permission to manage clans.";
//                    return RedirectToPage();
//                }

//                if (string.IsNullOrWhiteSpace(ClanTag))
//                {
//                    TempData["ErrorMessage"] = "Please select a clan to delete.";
//                    return RedirectToPage();
//                }

//                var result = await _applicationService.DeleteClanAsync(ClanTag);
//                if (result.Success)
//                {
//                    TempData["SuccessMessage"] = result.Message;
//                }
//                else
//                {
//                    TempData["ErrorMessage"] = result.Message;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting clan with tag: {ClanTag}", ClanTag);
//                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the clan. Please try again.";
//            }

//            return RedirectToPage();
//        }
//    }

//    public class PlayerSpreadsheetRow
//    {
//        public int PlayerID { get; set; }
//        public string PlayerTag { get; set; } = string.Empty;
//        public string PlayerName { get; set; } = string.Empty;
//        public string Status { get; set; } = string.Empty;
//        public string ClanName { get; set; } = string.Empty;
//        public Dictionary<string, PlayerWarDataCell> WarData { get; set; } = new();
//    }

//    public class PlayerWarDataCell
//    {
//        public int Fame { get; set; }
//        public int DecksUsed { get; set; }
//        public bool HasData { get; set; }
//        public string DisplayText => HasData ? $"{Fame}/{DecksUsed}" : "-";
//    }
//}
