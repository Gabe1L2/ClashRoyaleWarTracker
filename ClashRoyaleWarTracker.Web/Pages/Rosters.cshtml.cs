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
            await LoadUserPermissionsAsync();
            if (!CanModifyPlayerData)
            {
                return Forbid();
            }

            Is5kTrophies = is5k;

            var clansResult = await _applicationService.GetAllClansAsync();
            if (clansResult.Success && clansResult.Data != null)
            {
                Clans = clansResult.Data.ToList();
            }

            var avgResult = await _applicationService.GetAllRosterAssignmentDTOsAsync();
            if (avgResult.Success && avgResult.Data != null)
            {
                RosterAssignments = avgResult.Data.ToList();
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

        //public async Task<IActionResult> OnPostUpdateRosterByFameAverageAsync()
        //{
        //    await LoadUserPermissionsAsync();
        //    if (!CanModifyPlayerData)
        //    {
        //        return Forbid();
        //    }

        //}

        public async Task<JsonResult> OnPostUpdateRowAsync([FromBody] RosterUpdateModel model)
        {
            await LoadUserPermissionsAsync();
            if (!CanModifyPlayerData)
            {
                return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
            }

            _logger.LogInformation("Received UpdateRow for roster id {Id} (not persisted yet)", model.Id);

            // TODO: Implement persistence (upsert to RosterAssignments via ApplicationService/repository)
            return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
        }

        public async Task<JsonResult> OnPostSaveAllAsync([FromBody] List<RosterUpdateModel> updates)
        {
            await LoadUserPermissionsAsync();
            if (!CanModifyPlayerData)
            {
                return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
            }

            _logger.LogInformation("Received SaveAll for {Count} updates (not persisted yet)", updates?.Count ?? 0);

            // TODO: Implement bulk upsert
            return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
        }

        public async Task<JsonResult> OnPostBulkAssignAsync([FromBody] BulkAssignModel model)
        {
            await LoadUserPermissionsAsync();
            if (!CanModifyPlayerData)
            {
                return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
            }

            _logger.LogInformation("Received BulkAssign for {Count} roster ids to clan {ClanId} (not persisted)", model.RosterIds.Count, model.AssignedClanId);

            // TODO: Implement bulk assign
            return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
        }

        public async Task<JsonResult> OnPostBulkToggleInClanAsync([FromBody] List<int> rosterIds)
        {
            await LoadUserPermissionsAsync();
            if (!CanModifyPlayerData)
            {
                return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
            }

            _logger.LogInformation("Received BulkToggleInClan for {Count} roster ids (not persisted)", rosterIds.Count);

            // TODO: Implement bulk toggle
            return new JsonResult(new { success = false, message = "Not implemented: server-side roster persistence" }) { StatusCode = 501 };
        }

        public async Task<JsonResult> OnPostCopyPreviousWeekAsync()
        {
            await LoadUserPermissionsAsync();
            if (!CanModifyPlayerData)
            {
                return new JsonResult(new { success = false, message = "Access denied" }) { StatusCode = 403 };
            }

            _logger.LogInformation("Received CopyPreviousWeek (not implemented)");

            // TODO: Implement copy logic using roster history/assignments
            return new JsonResult(new { success = false, message = "Not implemented: copy previous week" }) { StatusCode = 501 };
        }
    }
}
