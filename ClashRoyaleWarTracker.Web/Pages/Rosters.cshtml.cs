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
        private readonly IApplicationService _applicationService;
        private readonly ILogger<RostersModel> _logger;

        public RostersModel(IApplicationService applicationService, ILogger<RostersModel> logger, IUserRoleService userRoleService) : base(userRoleService)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        public List<RosterAssignmentDTO> RosterAssignments { get; set; } = new();
        public IList<Clan> Clans { get; set; } = new List<Clan>();
        [BindProperty]
        public bool Is5kTrophies { get; set; } = true;

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

                var rosterResult = await _applicationService.GetAllRosterAssignmentDTOsAsync();
                if (rosterResult.Success && rosterResult.Data != null)
                {
                    RosterAssignments = rosterResult.Data.ToList();
                    _logger.LogDebug("Successfully loaded {RosterCount} roster assignments", RosterAssignments.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to load roster assignments: {Message}", rosterResult.Message);
                    TempData["ErrorMessage"] = "Failed to load roster assignments. Please try refreshing the page.";
                    RosterAssignments = new List<RosterAssignmentDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while loading the roster page");
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the page. Please try again.";
                
                Clans = new List<Clan>();
                RosterAssignments = new List<RosterAssignmentDTO>();
            }

            return Page();
        }

        // Handler stubs - not yet implemented server-side persistence
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

                _logger.LogInformation("Received UpdateRow for roster id {Id} (not persisted yet)", model.Id);

                // TODO: Implement persistence (upsert to RosterAssignments via ApplicationService/repository)
                return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating roster row");
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }

        public async Task<JsonResult> OnPostSaveAllAsync([FromBody] List<RosterUpdateModel> updates)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to save all roster updates without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                if (updates == null)
                {
                    _logger.LogWarning("Received null updates list for SaveAll request");
                    return new JsonResult(new { success = false, message = "Invalid request data" }) { StatusCode = 400 };
                }

                _logger.LogInformation("Received SaveAll for {Count} updates (not persisted yet)", updates.Count);

                // TODO: Implement bulk upsert
                return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while saving all roster updates");
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }

        public async Task<JsonResult> OnPostBulkAssignAsync([FromBody] BulkAssignModel model)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to bulk assign without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                if (model == null || model.RosterIds == null)
                {
                    _logger.LogWarning("Received invalid model for BulkAssign request");
                    return new JsonResult(new { success = false, message = "Invalid request data" }) { StatusCode = 400 };
                }

                _logger.LogInformation("Received BulkAssign for {Count} roster ids to clan {ClanId} (not persisted)", model.RosterIds.Count, model.AssignedClanId);

                // TODO: Implement bulk assign
                return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while bulk assigning roster entries");
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }

        public async Task<JsonResult> OnPostBulkToggleInClanAsync([FromBody] List<int> rosterIds)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to bulk toggle in-clan without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                if (rosterIds == null)
                {
                    _logger.LogWarning("Received null roster IDs list for BulkToggleInClan request");
                    return new JsonResult(new { success = false, message = "Invalid request data" }) { StatusCode = 400 };
                }

                _logger.LogInformation("Received BulkToggleInClan for {Count} roster ids (not persisted)", rosterIds.Count);

                // TODO: Implement bulk toggle
                return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while bulk toggling in-clan status");
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }

        public async Task<JsonResult> OnPostCopyPreviousWeekAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to copy previous week without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                _logger.LogInformation("Received CopyPreviousWeek request (not implemented)");

                // TODO: Implement copy logic using roster history/assignments
                return new JsonResult(new { success = false, message = "Not implemented: copy previous week" }) { StatusCode = 501 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while copying previous week roster");
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }
    }
}
