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

        public async Task OnGetAsync()
        {
            try
            {
                var result = await _applicationService.GetAllPlayerAveragesAsync();
                if (result.Success && result.Data != null)
                {
                    PlayerAverages = result.Data.ToList();
                }
                else
                {
                    _logger.LogWarning("Failed to load player averages: {Message}", result.Message);
                    PlayerAverages = new List<PlayerAverageDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading player averages");
                PlayerAverages = new List<PlayerAverageDTO>();
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
