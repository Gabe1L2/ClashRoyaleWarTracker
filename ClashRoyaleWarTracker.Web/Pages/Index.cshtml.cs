using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
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

        public IndexModel(IApplicationService applicationService, ILogger<IndexModel> logger)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        public IList<PlayerAverageDTO> PlayerAverages { get; set; } = new List<PlayerAverageDTO>();
        public IList<Clan> AllClans { get; set; } = new List<Clan>();

        public async Task OnGetAsync()
        {
            try
            {
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
            }
        }

        public async Task<IActionResult> OnPostWeeklyUpdateAsync()
        {
            try
            {
                var result = await _applicationService.WeeklyUpdateAsync();
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
                _logger.LogError(ex, "Error during weekly update");
                TempData["ErrorMessage"] = "An unexpected error occurred during the weekly update.";
            }

            return RedirectToPage();
        }
    }
}
