using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleWarTracker.Web.Pages
{
    [Authorize]
    public class RostersModel : BasePageModel
    {

        public RostersModel(IUserRoleService userRoleService, IApplicationService applicationService, ILogger<RostersModel> logger) : base(userRoleService, applicationService, logger)
        {
        }

        public List<RosterAssignmentDTO> RosterAssignments { get; set; } = new();
        public IList<Clan> Clans { get; set; } = new List<Clan>();
        public List<(int SeasonId, int WeekIndex, string Display)> AvailableSeasonWeeks { get; set; } = new();
        
        [BindProperty]
        public bool Is5kTrophies { get; set; } = true;
        
        [BindProperty(SupportsGet = true)]
        public int SelectedSeasonId { get; set; } = 999;
        
        [BindProperty(SupportsGet = true)]
        public int SelectedWeekIndex { get; set; } = 999;

        public async Task<IActionResult> OnGetAsync(bool is5k = true)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to access roster page without proper permissions", User.Identity?.Name);
                    return Forbid();
                }

                Is5kTrophies = is5k;

                var clansResult = await _applicationService.GetAllClansAsync();
                if (clansResult.Success && clansResult.Data != null)
                {
                    Clans = clansResult.Data.ToList();
                    _logger.LogDebug("Successfully loaded {ClanCount} clans", Clans.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to load clans: {Message}", clansResult.Message);
                    TempData["ErrorMessage"] = "Failed to load clans. Please try refreshing the page.";
                    Clans = new List<Clan>();
                }

                // Load available season/weeks for dropdown
                var seasonWeeksResult = await _applicationService.GetAvailableRosterSeasonWeeksAsync();
                if (seasonWeeksResult.Success && seasonWeeksResult.Data != null)
                {
                    AvailableSeasonWeeks = seasonWeeksResult.Data
                        .Select(sw => (
                            SeasonId: sw.SeasonId,
                            WeekIndex: sw.WeekIndex,
                            Display: sw.SeasonId == 999 && sw.WeekIndex == 999 
                                ? "Current Roster (999-999)" 
                                : $"Season {sw.SeasonId} - Week {sw.WeekIndex}"
                        ))
                        .ToList();
                    _logger.LogDebug("Successfully loaded {Count} available season/weeks", AvailableSeasonWeeks.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to load available season/weeks: {Message}", seasonWeeksResult.Message);
                    AvailableSeasonWeeks = new List<(int, int, string)> { (999, 999, "Current Roster (999-999)") };
                }

                // Load roster assignments for the selected season/week
                var rosterResult = await _applicationService.GetRosterAssignmentsBySeasonWeekAsync(SelectedSeasonId, SelectedWeekIndex);
                if (rosterResult.Success && rosterResult.Data != null)
                {
                    RosterAssignments = rosterResult.Data.ToList();
                    _logger.LogDebug("Successfully loaded {RosterCount} roster assignments for Season {SeasonId}, Week {WeekIndex}", 
                        RosterAssignments.Count, SelectedSeasonId, SelectedWeekIndex);
                }
                else
                {
                    _logger.LogWarning("Failed to load roster assignments for Season {SeasonId}, Week {WeekIndex}: {Message}", 
                        SelectedSeasonId, SelectedWeekIndex, rosterResult.Message);
                    TempData["ErrorMessage"] = $"Failed to load roster assignments for the selected week. Please try refreshing the page.";
                    RosterAssignments = new List<RosterAssignmentDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while loading the roster page");
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the page. Please try again.";
                
                Clans = new List<Clan>();
                RosterAssignments = new List<RosterAssignmentDTO>();
                AvailableSeasonWeeks = new List<(int, int, string)> { (999, 999, "Current Roster (999-999)") };
            }

            return Page();
        }

        public class RosterUpdateModel
        {
            public int Id { get; set; }
            public int? AssignedClanId { get; set; }
            public bool IsInClan { get; set; }
        }

        public class BulkAssignModel
        {
            public List<int> RosterIds { get; set; } = new();
            public int? AssignedClanId { get; set; }
        }

        public async Task<IActionResult> OnPostUpdateRosterByFameAverageAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    TempData["ErrorMessage"] = "You don't have permission to modify player data.";
                    _logger.LogWarning("User {UserName} attempted to update roster by fame average without proper permissions", User.Identity?.Name);
                    return RedirectToPage();
                }

                _logger.LogInformation("Auto roster by fame average requested by user {UserName}", User.Identity?.Name);

                var result = await _applicationService.UpdateRosterByFameAverageAsync();
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    _logger.LogInformation("Auto roster completed successfully: {Message}", result.Message);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    _logger.LogWarning("Auto roster failed: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing auto roster by fame average");
                TempData["ErrorMessage"] = "An unexpected error occurred while creating the roster. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateInClanStatusAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    TempData["ErrorMessage"] = "You don't have permission to modify player data.";
                    _logger.LogWarning("User {UserName} attempted to update IsInClan status without proper permissions", User.Identity?.Name);
                    return RedirectToPage();
                }

                _logger.LogInformation("IsInClan status update requested by user {UserName}", User.Identity?.Name);

                var result = await _applicationService.UpdateRosterInClanStatusAsync();
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    _logger.LogInformation("IsInClan status update completed successfully: {Message}", result.Message);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    _logger.LogWarning("IsInClan status update failed: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating IsInClan status");
                TempData["ErrorMessage"] = "An unexpected error occurred while updating IsInClan status. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateInClanStatusForClanAsync(int? clanId)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    TempData["ErrorMessage"] = "You don't have permission to modify player data.";
                    _logger.LogWarning("User {UserName} attempted to update IsInClan status for clan without proper permissions", User.Identity?.Name);
                    return RedirectToPage();
                }

                var clanName = clanId.HasValue ? $"ClanID {clanId}" : "unassigned players";
                _logger.LogInformation("IsInClan status update for {ClanName} requested by user {UserName}", clanName, User.Identity?.Name);

                var result = await _applicationService.UpdateRosterInClanStatusForClanAsync(clanId);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    _logger.LogInformation("IsInClan status update for {ClanName} completed successfully: {Message}", clanName, result.Message);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    _logger.LogWarning("IsInClan status update for {ClanName} failed: {Message}", clanName, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating IsInClan status for ClanID {ClanId}", clanId);
                TempData["ErrorMessage"] = "An unexpected error occurred while updating IsInClan status. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnPostUpdateRowAsync([FromBody] RosterUpdateModel model)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to update roster row without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                if (model == null)
                {
                    _logger.LogWarning("Received null model for UpdateRow request");
                    return new JsonResult(new { success = false, message = "Invalid request data" }) { StatusCode = 400 };
                }

                _logger.LogInformation("Updating roster assignment {Id} to clan {ClanId} by user {UserName}", 
                    model.Id, model.AssignedClanId, User.Identity?.Name);

                var result = await _applicationService.UpdateRosterAssignmentAsync(model.Id, model.AssignedClanId, Username);
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully updated roster assignment {Id}", model.Id);
                    return new JsonResult(new { success = true, message = "Roster assignment updated successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to update roster assignment {Id}: {Message}", model.Id, result.Message);
                    return new JsonResult(new { success = false, message = result.Message }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating roster row {Id}", model?.Id);
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }
    }
}
