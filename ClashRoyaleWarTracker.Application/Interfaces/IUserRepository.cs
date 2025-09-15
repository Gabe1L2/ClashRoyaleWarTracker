using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IList<string>> GetUserRolesAsync(ClaimsPrincipal user);
        Task<IList<string>> GetUserRolesAsync(string userId);
        string? GetUserIdAsync(ClaimsPrincipal user);
        Task<bool> UserExistsAsync(string userId);
        Task<IList<UserWithRoles>> GetAllUsersWithRolesAsync();
        Task<IList<string>> GetAllRolesAsync();
        Task<IdentityResult> CreateUserAsync(string username, string password, string role);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> UpdateUserRoleAsync(string userId, string newRole);
        Task<IdentityResult> ChangePasswordAsync(string userId, string newPassword);
    }
}