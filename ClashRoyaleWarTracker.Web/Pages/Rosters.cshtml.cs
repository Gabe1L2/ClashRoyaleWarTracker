using ClashRoyaleWarTracker.Application.Interfaces;
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
        private readonly ILogger<IndexModel> _logger;

        public RostersModel(IApplicationService applicationService, ILogger<IndexModel> logger, IUserRoleService userRoleService) : base(userRoleService)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
