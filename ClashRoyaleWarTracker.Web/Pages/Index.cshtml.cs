using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClashRoyaleWarTracker.Web.Pages.Shared;

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

        public IList<PlayerAverageDTO> PlayerAverages { get; set; } = new List<PlayerAverageDTO>();
        public IList<Clan> AllClans { get; set; } = new List<Clan>();
        

        [BindProperty]
        public string ClanTag { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();

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
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
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
                    return RedirectToPage();
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBacklogUpdateAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();
                
                if (!CanUpdateWarData)
                {
                    TempData["ErrorMessage"] = "You don't have permission to update data.";
                    return RedirectToPage();
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddClanAsync()
        {
            try
            {
                await LoadUserPermissionsAsync();
                
                if (!CanManageClans)
                {
                    TempData["ErrorMessage"] = "You don't have permission to manage clans.";
                    return RedirectToPage();
                }

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
            try
            {
                await LoadUserPermissionsAsync();
                
                if (!CanManageClans)
                {
                    TempData["ErrorMessage"] = "You don't have permission to manage clans.";
                    return RedirectToPage();
                }

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
