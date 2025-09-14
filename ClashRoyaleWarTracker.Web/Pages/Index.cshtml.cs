using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleWarTracker.Web.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApplicationService _applicationService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IUserRoleService _userRoleService;

        public IndexModel(
            IApplicationService applicationService, 
            ILogger<IndexModel> logger, 
            IUserRoleService userRoleService)
        {
            _applicationService = applicationService;
            _logger = logger;
            _userRoleService = userRoleService;
        }

        public IList<PlayerAverageDTO> PlayerAverages { get; set; } = new List<PlayerAverageDTO>();
        public IList<Clan> AllClans { get; set; } = new List<Clan>();
        
        // Replace individual bool properties with role-based properties
        public UserRole CurrentUserRole { get; set; }
        public bool CanManageClans => RolePermissions.HasPermission(CurrentUserRole, Permissions.ManageClans);
        public bool CanViewStatistics => RolePermissions.HasPermission(CurrentUserRole, Permissions.ViewStatistics);
        public bool CanUpdateWarData => RolePermissions.HasPermission(CurrentUserRole, Permissions.UpdateWarData);
        public bool CanManageUsers => RolePermissions.HasPermission(CurrentUserRole, Permissions.ManageUsers);
        public bool CanModifyPlayerData => RolePermissions.HasPermission(CurrentUserRole, Permissions.ModifyPlayerData);

        [BindProperty]
        public string ClanTag { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                // Get current user's role
                var getUserRoleResult = await _userRoleService.GetUserRoleAsync(User);
                if (getUserRoleResult.Success)
                {
                    CurrentUserRole = getUserRoleResult.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to get user role: {Message}", getUserRoleResult.Message);
                    CurrentUserRole = UserRole.Guest;
                }

                var playerAveragesResult = await _applicationService.GetAllPlayerAveragesAsync();
                if (playerAveragesResult.Success && playerAveragesResult.Data != null)
                {
                    PlayerAverages = playerAveragesResult.Data.ToList();
                }
                else
                {
                    _logger.LogWarning("Failed to load player averages: {Message}", playerAveragesResult.Message);
                    PlayerAverages = new List<PlayerAverageDTO>();
                }

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
                PlayerAverages = new List<PlayerAverageDTO>();
                AllClans = new List<Clan>();
                CurrentUserRole = UserRole.Guest;
            }
        }

        public async Task<IActionResult> OnPostWeeklyUpdateAsync()
        {
            // Check permission
            var hasPermissionResult = await _userRoleService.HasPermissionAsync(User, Permissions.UpdateWarData);
            if (!hasPermissionResult.Success || !hasPermissionResult.Data)
            {
                TempData["ErrorMessage"] = "You don't have permission to update data.";
                return RedirectToPage();
            }

            try
            {
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBacklogUpdateAsync()
        {
            // Check permission
            var hasPermissionResult = await _userRoleService.HasPermissionAsync(User, Permissions.UpdateWarData);
            if (!hasPermissionResult.Success || !hasPermissionResult.Data)
            {
                TempData["ErrorMessage"] = "You don't have permission to update data.";
                return RedirectToPage();
            }

            try
            {
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddClanAsync()
        {
            // Check permission using the service
            var hasPermissionResult = await _userRoleService.HasPermissionAsync(User, Permissions.ManageClans);
            if (!hasPermissionResult.Success || !hasPermissionResult.Data)
            {
                TempData["ErrorMessage"] = "You don't have permission to update data.";
                return RedirectToPage();
            }

            try
            {
                var result = await _applicationService.AddClanAsync(ClanTag ?? string.Empty);
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
                _logger.LogError(ex, "Error adding clan with tag: {ClanTag}", ClanTag);
                TempData["ErrorMessage"] = "An unexpected error occurred while adding the clan. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteClanAsync()
        {
            // Check permission using the service
            var hasPermissionResult = await _userRoleService.HasPermissionAsync(User, Permissions.ManageClans);
            if (!hasPermissionResult.Success || !hasPermissionResult.Data)
            {
                TempData["ErrorMessage"] = "You don't have permission to update data.";
                return RedirectToPage();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(ClanTag))
                {
                    TempData["ErrorMessage"] = "Please select a clan to delete.";
                    return RedirectToPage();
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

            return RedirectToPage();
        }
    }
}
