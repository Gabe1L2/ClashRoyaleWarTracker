using System.Security.Claims;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IList<string>> GetUserRolesAsync(ClaimsPrincipal user);
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<string?> GetUserIdAsync(ClaimsPrincipal user);
        Task<bool> UserExistsAsync(string userId);
    }
}