using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleWarTracker.Web.Pages.Shared
{
    public abstract class BasePageModel : PageModel
    {
        protected readonly IUserRoleService _userRoleService;

        public BasePageModel(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        public UserRole CurrentUserRole { get; set; }
        public bool CanManageUsers => RolePermissions.HasPermission(CurrentUserRole, Permissions.ManageUsers);
        public bool CanManageClans => RolePermissions.HasPermission(CurrentUserRole, Permissions.ManageClans);
        public bool CanUpdateWarData => RolePermissions.HasPermission(CurrentUserRole, Permissions.UpdateWarData);
        public bool CanViewStatistics => RolePermissions.HasPermission(CurrentUserRole, Permissions.ViewStatistics);
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
    }
}