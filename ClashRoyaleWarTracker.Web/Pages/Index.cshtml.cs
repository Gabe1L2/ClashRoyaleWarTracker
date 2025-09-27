using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Infrastructure.Repositories;
using ClashRoyaleWarTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleWarTracker.Web.Pages
{
    [Authorize]
    public class IndexModel : BasePageModel
    {
        private readonly IApplicationService _applicationService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IApplicationService applicationService, ILogger<IndexModel> logger, IUserRoleService userRoleService) : base(userRoleService)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        public List<GroupedPlayerWarHistoryDTO> GroupedPlayerWarHistories { get; set; } = new();
        public List<PlayerSpreadsheetRow> PlayerRows { get; set; } = new();
        public List<string> SeasonWeekHeaders { get; set; } = new();
        public IList<Clan> AllClans { get; set; } = new List<Clan>();
        public List<string> AllStatuses { get; set; } = new();
        public int TotalRecords { get; set; }
        public Dictionary<int, PlayerAverageDTO> PlayerAverages { get; set; } = new();

        [BindProperty]
        public bool Is5kTrophies { get; set; } = true;
        [BindProperty]
        public string ClanTag { get; set; } = string.Empty;
        [BindProperty]
        public int WeeksToFetch { get; set; } = 1;

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
                    AllClans = new List<Clan>();
                    PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
                    TotalRecords = 0;
                }

                // Load all clans for filtering
                var clansResult = await _applicationService.GetAllClansAsync();
                if (clansResult.Success && clansResult.Data != null)
                {
                    AllClans = clansResult.Data.ToList();
                }
                else
                {
                    _logger.LogWarning("Failed to load clans: {Message}", clansResult.Message);
                    AllClans = new List<Clan>();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading war histories data");
                GroupedPlayerWarHistories = new List<GroupedPlayerWarHistoryDTO>();
                PlayerRows = new List<PlayerSpreadsheetRow>();
                SeasonWeekHeaders = new List<string>();
                AllClans = new List<Clan>();
                PlayerAverages = new Dictionary<int, PlayerAverageDTO>();
                TotalRecords = 0;
                TempData["ErrorMessage"] = "An error occurred while loading the war histories.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostToggleTrophyLevelAsync()
        {
            return RedirectToPage("Index", new { is5k = Is5kTrophies });
        }

        private async Task LoadPlayerAveragesAsync()
        {
            try
            {
                var averagesResult = await _applicationService.GetAllPlayerAverageDTOsAsync();
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

            AllStatuses = GroupedPlayerWarHistories
                .Select(g => g.Status)
                .Where(status => !string.IsNullOrEmpty(status))
                .Distinct()
                .OrderBy(status => status)
                .ToList();

            var playerGroups = GroupedPlayerWarHistories
                .GroupBy(g => new { g.PlayerID, g.PlayerTag, g.PlayerName, g.Status, g.Notes })
                .OrderBy(pg => pg.Key.PlayerName);

            PlayerRows = new List<PlayerSpreadsheetRow>();

            foreach (var playerGroup in playerGroups)
            {
                var row = new PlayerSpreadsheetRow
                {
                    PlayerID = playerGroup.Key.PlayerID,
                    PlayerTag = playerGroup.Key.PlayerTag,
                    PlayerName = playerGroup.Key.PlayerName,
                    Status = playerGroup.Key.Status,
                    Notes = playerGroup.Key.Notes,
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

            // ADDED: Sort PlayerRows by Fame Attack Average descending (default sort)
            PlayerRows = PlayerRows
                .OrderByDescending(row =>
                {
                    // Get the fame attack average for this player
                    if (PlayerAverages.ContainsKey(row.PlayerID))
                    {
                        return PlayerAverages[row.PlayerID].FameAttackAverage;
                    }
                    return 0; // Default to 0 if no average found
                })
                .ThenBy(row => row.PlayerName) // Secondary sort by name for consistency
                .ToList();
        }

        public async Task<IActionResult> OnPostUpdatePlayerStatusAsync(int playerId, string status)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to update player status without proper permissions", User.Identity?.Name);
                    return Forbid();
                }

                var result = await _applicationService.UpdatePlayerStatusAsync(playerId, status);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player status");
                TempData["ErrorMessage"] = "An error occurred while updating player status.";
                return RedirectToPage("Index", new { is5k = Is5kTrophies });
            }
        }

        public async Task<JsonResult> OnGetPlayerWarHistoriesAsync(int playerId)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanViewWarHistory)
                {
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                var result = await _applicationService.GetPlayerWarHistoriesByPlayerIdAsync(playerId);
                if (result.Success)
                {
                    return new JsonResult(new { success = true, data = result.Data });
                }
                else
                {
                    return new JsonResult(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player war histories");
                return new JsonResult(new { success = false, message = "An error occurred while retrieving war histories." });
            }
        }

        public async Task<JsonResult> OnPostUpdateWarHistoryAsync(int warHistoryId, int fame, int decksUsed, int boatAttacks)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                var updateResult = await _applicationService.UpdatePlayerWarHistoryAsync(warHistoryId, fame, decksUsed, boatAttacks);
                if (!updateResult.Success)
                {
                    return new JsonResult(new { success = false, message = updateResult.Message });
                }

                var playerIdResult = await _applicationService.GetPlayerIdFromWarHistoryAsync(warHistoryId);
                if (!playerIdResult.Success)
                {
                    return new JsonResult(new { success = false, message = playerIdResult.Message });
                }

                var updateAveragesResult = await _applicationService.UpdatePlayerAverageAsync(playerIdResult.Data, 4);
                if (!updateAveragesResult.Success)
                {
                    _logger.LogWarning("Failed to update player averages after war history update: {Message}", updateAveragesResult.Message);
                }
                else
                {
                    _logger.LogInformation("Updated war history and recalculated player averages for PlayerID {PlayerId}", playerIdResult.Data);
                }

                return new JsonResult(new { success = updateAveragesResult.Success, message = updateAveragesResult.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating war history and player averages");
                return new JsonResult(new { success = false, message = "An error occurred while updating war history." });
            }
        }

        public async Task<IActionResult> OnPostWeeklyUpdateAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();

                if (!CanUpdateWarData)
                {
                    TempData["ErrorMessage"] = "You don't have permission to update data.";
                    return Forbid();
                }

                var result = await _applicationService.DataUpdateAsync(1);
                if (result.Success)
                {
                    _logger.LogInformation("Weekly update successful: {Message}", result.Message);
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    _logger.LogWarning("Weekly update failed: {Message}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during weekly update");
                TempData["ErrorMessage"] = "An unexpected error occurred during the weekly update.";
            }

            return RedirectToPage("Index", new { is5k = Is5kTrophies });
        }

        public async Task<IActionResult> OnPostBacklogUpdateAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();

                if (!CanUpdateWarData)
                {
                    TempData["ErrorMessage"] = "You don't have permission to update data.";
                    return Forbid();
                }

                var result = await _applicationService.DataUpdateAsync(10);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during backlog update");
                TempData["ErrorMessage"] = "An unexpected error occurred during the backlog update.";
            }

            return RedirectToPage("Index", new { is5k = Is5kTrophies });
        }

        public async Task<IActionResult> OnPostAddClanAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();

                if (!CanManageClans)
                {
                    TempData["ErrorMessage"] = "You don't have permission to manage clans.";
                    return Forbid();
                }

                if (WeeksToFetch < 0 || WeeksToFetch > 10)
                {
                    TempData["ErrorMessage"] = "Weeks to fetch must be between 0 and 10.";
                    return RedirectToPage();
                }

                var addResult = await _applicationService.AddClanClanHistoryPlayerHistoryAsync(ClanTag ?? string.Empty, WeeksToFetch);
                if (!addResult.Success)
                {
                    TempData["ErrorMessage"] = addResult.Message;
                }

                TempData["SuccessMessage"] = "Clan added and data updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding clan with tag: {ClanTag}", ClanTag);
                TempData["ErrorMessage"] = "An unexpected error occurred while adding the clan. Please try again.";
            }

            return RedirectToPage("Index", new { is5k = Is5kTrophies });
        }

        public async Task<IActionResult> OnPostDeleteClanAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();

                if (!CanManageClans)
                {
                    TempData["ErrorMessage"] = "You don't have permission to manage clans.";
                    return Forbid();
                }

                if (string.IsNullOrWhiteSpace(ClanTag))
                {
                    TempData["ErrorMessage"] = "Please select a clan to delete.";
                    return RedirectToPage("Index", new { is5k = Is5kTrophies });
                }

                var result = await _applicationService.DeleteClanAsync(ClanTag);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting clan with tag: {ClanTag}", ClanTag);
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the clan. Please try again.";
            }

            return RedirectToPage("Index", new { is5k = Is5kTrophies });
        }

        public async Task<IActionResult> OnPostPlayerAveragesUpdateAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    TempData["ErrorMessage"] = "You don't have permission to modify player data.";
                    return Forbid();
                }

                // Use the Is5kTrophies bind property directly
                var result = await _applicationService.UpdateAllActivePlayerAverages(4, Is5kTrophies);

                if (result.Success)
                {
                    var trophyLevel = Is5kTrophies ? "5k+" : "sub-5k";
                    TempData["SuccessMessage"] = $"Player averages updated successfully for {trophyLevel} players.";
                    _logger.LogInformation("Player averages updated successfully for {TrophyLevel} players", trophyLevel);
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to update player averages: {result.Message}";
                    _logger.LogWarning("Failed to update player averages for {TrophyLevel} players: {Message}",
                        Is5kTrophies ? "5k+" : "sub-5k", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player averages for {TrophyLevel} players", Is5kTrophies ? "5k+" : "sub-5k");
                TempData["ErrorMessage"] = "An unexpected error occurred while updating player averages. Please try again.";
            }

            return RedirectToPage("Index", new { is5k = Is5kTrophies });
        }

        public async Task<JsonResult> OnPostUpdatePlayerNotesAsync(int playerId, string? notes)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                if (!string.IsNullOrEmpty(notes) && notes.Length > 100)
                {
                    return new JsonResult(new { success = false, message = "Notes must be 100 characters or fewer." });
                }

                var result = await _applicationService.UpdatePlayerNotesAsync(playerId, notes);
                if (result.Success)
                {
                    return new JsonResult(new { success = true, message = result.Message });
                }
                else
                {
                    return new JsonResult(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player notes for PlayerID {PlayerId}", playerId);
                return new JsonResult(new { success = false, message = "An unexpected error occurred while updating notes." });
            }
        }
    }

    public class PlayerSpreadsheetRow
    {
        public int PlayerID { get; set; }
        public string PlayerTag { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
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
