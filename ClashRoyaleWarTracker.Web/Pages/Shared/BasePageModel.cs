using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleWarTracker.Web.Pages.Shared
{
    public abstract class BasePageModel : PageModel
    {
        protected readonly IUserRoleService _userRoleService;
        protected readonly IApplicationService _applicationService;
        protected readonly ILogger _logger;
        public BasePageModel(IUserRoleService userRoleService, IApplicationService applicationService, ILogger logger)
        {
            _userRoleService = userRoleService;
            _applicationService = applicationService;
            _logger = logger;
        }

        protected string Username => User.Identity?.Name ?? "Unknown";
        public UserRole CurrentUserRole { get; set; }
        public bool CanManageUsers => RolePermissions.HasPermission(CurrentUserRole, Permissions.ManageUsers);
        public bool CanManageClans => RolePermissions.HasPermission(CurrentUserRole, Permissions.ManageClans);
        public bool CanUpdateWarData => RolePermissions.HasPermission(CurrentUserRole, Permissions.UpdateWarData);
        public bool CanModifyPlayerData => RolePermissions.HasPermission(CurrentUserRole, Permissions.ModifyPlayerData);
        public bool CanViewWarHistory => RolePermissions.HasPermission(CurrentUserRole, Permissions.ViewWarHistory);


        protected async Task LoadUserPermissionsAsync()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userRoleResult = await _userRoleService.GetUserRoleAsync(User);
                CurrentUserRole = userRoleResult.Success ? userRoleResult.Data : UserRole.Guest;
            }
            else
            {
                CurrentUserRole = UserRole.Guest;
            }
        }

        // Shared Player Actions Modal Handlers
        public virtual async Task<JsonResult> OnGetPlayerWarHistoriesAsync(int playerId)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to retrieve player war histories without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                _logger.LogInformation("Retrieving war histories for player {PlayerId}", playerId);

                var result = await _applicationService.GetPlayerWarHistoriesByPlayerIdAsync(playerId);

                if (result.Success && result.Data != null)
                {
                    var histories = result.Data.Select(h => new
                    {
                        id = h.ID,
                        seasonID = h.SeasonID,
                        weekIndex = h.WeekIndex,
                        clanName = h.ClanName ?? "Unknown",
                        fame = h.Fame,
                        decksUsed = h.DecksUsed,
                        boatAttacks = h.BoatAttacks
                    });

                    return new JsonResult(new { success = true, data = histories });
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve war histories for player {PlayerId}: {Message}", playerId, result.Message);
                    return new JsonResult(new { success = false, message = result.Message ?? "Failed to retrieve war histories" }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving war histories for player {PlayerId}", playerId);
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }

        public virtual async Task<IActionResult> OnPostUpdatePlayerStatusAsync(int playerId, string status)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to update player status without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                _logger.LogInformation("Updating player {PlayerId} status to {Status} by user {UserName}",
                    playerId, status, User.Identity?.Name);

                var result = await _applicationService.UpdatePlayerStatusAsync(playerId, status, Username);

                if (result.Success)
                {
                    _logger.LogInformation("Successfully updated player {PlayerId} status to {Status}", playerId, status);
                    return new JsonResult(new { success = true, message = "Player status updated successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to update player {PlayerId} status: {Message}", playerId, result.Message);
                    return new JsonResult(new { success = false, message = result.Message }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating player status for {PlayerId}", playerId);
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }

        public virtual async Task<IActionResult> OnPostUpdatePlayerNotesAsync(int playerId, string? notes)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to update player notes without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                _logger.LogInformation("Updating notes for player {PlayerId} by user {UserName}", playerId, User.Identity?.Name);

                var result = await _applicationService.UpdatePlayerNotesAsync(playerId, notes, Username);

                if (result.Success)
                {
                    _logger.LogInformation("Successfully updated notes for player {PlayerId}", playerId);
                    return new JsonResult(new { success = true, message = "Player notes updated successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to update notes for player {PlayerId}: {Message}", playerId, result.Message);
                    return new JsonResult(new { success = false, message = result.Message }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating notes for player {PlayerId}", playerId);
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }

        public virtual async Task<IActionResult> OnPostUpdateWarHistoryAsync(int warHistoryId, int fame, int decksUsed, int boatAttacks)
        {
            try
            {
                await LoadUserPermissionsAsync();
                if (!CanModifyPlayerData)
                {
                    _logger.LogWarning("User {UserName} attempted to update war history without proper permissions", User.Identity?.Name);
                    return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
                }

                _logger.LogInformation("Updating war history {WarHistoryId} by user {UserName}",
                    warHistoryId, User.Identity?.Name);

                var result = await _applicationService.UpdatePlayerWarHistoryAsync(warHistoryId, fame, decksUsed, boatAttacks, Username);

                if (result.Success)
                {
                    _logger.LogInformation("Successfully updated war history {WarHistoryId}", warHistoryId);
                    return new JsonResult(new { success = true, message = "War history updated successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to update war history {WarHistoryId}: {Message}", warHistoryId, result.Message);
                    return new JsonResult(new { success = false, message = result.Message }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating war history {WarHistoryId}", warHistoryId);
                return new JsonResult(new { success = false, message = "An unexpected error occurred" }) { StatusCode = 500 };
            }
        }
    }
}