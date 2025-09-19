using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClashRoyaleWarTracker.Web.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly IUserRoleService _userRoleService;

        public NavigationViewComponent(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new NavigationViewModel();

            var user = ViewContext.HttpContext.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userRoleResult = await _userRoleService.GetUserRoleAsync(user);
                var currentUserRole = userRoleResult.Success 
                    ? userRoleResult.Data 
                    : UserRole.Guest;

                model.CanManageUsers = RolePermissions.HasPermission(currentUserRole, Permissions.ManageUsers);
                model.CanManageClans = RolePermissions.HasPermission(currentUserRole, Permissions.ManageClans);
                model.CanUpdateWarData = RolePermissions.HasPermission(currentUserRole, Permissions.UpdateWarData);
                model.CanViewWarHistory = RolePermissions.HasPermission(currentUserRole, Permissions.ViewWarHistory);
            }

            return View("MainNavigation", model);
        }
    }

    public class NavigationViewModel
    {
        public bool CanManageUsers { get; set; }
        public bool CanManageClans { get; set; }
        public bool CanUpdateWarData { get; set; }
        public bool CanViewWarHistory { get; set; }
    }
}