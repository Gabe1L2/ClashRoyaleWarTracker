using ClashRoyaleWarTracker.Application.Models;
using System.Security.Claims;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IUserRoleService
    {
        // Role Permission Logic
        Task<ServiceResult<UserRole>> GetUserRoleAsync(ClaimsPrincipal user);
        Task<ServiceResult<bool>> HasPermissionAsync(ClaimsPrincipal user, string permission);
        Task<ServiceResult<bool>> HasRoleOrHigherAsync(ClaimsPrincipal user, UserRole minimumRole);
        Task<ServiceResult<IList<string>>> GetUserRolesAsync(ClaimsPrincipal user);
        Task<ServiceResult<UserRole>> GetHighestUserRoleAsync(ClaimsPrincipal user);

        // User management methods
        Task<ServiceResult<IList<UserWithRoles>>> GetAllUsersWithRolesAsync();
        Task<ServiceResult<IList<string>>> GetAllRolesAsync();
        Task<ServiceResult> CreateUserAsync(string username, string password, string role);
        Task<ServiceResult> DeleteUserAsync(string userId);
        Task<ServiceResult> UpdateUserRoleAsync(string userId, string newRole);
        Task<ServiceResult> ChangePasswordAsync(string userId, string newPassword);
    }
}
