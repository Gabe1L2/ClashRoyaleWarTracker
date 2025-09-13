using ClashRoyaleWarTracker.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserManager<IdentityUser> userManager, ILogger<UserRepository> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IList<string>> GetUserRolesAsync(ClaimsPrincipal user)
        {
            try
            {
                var identityUser = await _userManager.GetUserAsync(user);
                if (identityUser == null)
                {
                    _logger.LogWarning("User not found in database for ClaimsPrincipal");
                    return new List<string>();
                }

                var roles = await _userManager.GetRolesAsync(identityUser);
                _logger.LogDebug("Retrieved {RoleCount} roles for user {UserName}", roles.Count, identityUser.UserName);
                
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for ClaimsPrincipal");
                return new List<string>();
            }
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var identityUser = await _userManager.FindByIdAsync(userId);
                if (identityUser == null)
                {
                    _logger.LogWarning("User not found in database for ID {UserId}", userId);
                    return new List<string>();
                }

                var roles = await _userManager.GetRolesAsync(identityUser);
                _logger.LogDebug("Retrieved {RoleCount} roles for user {UserName} ({UserId})", roles.Count, identityUser.UserName, userId);
                
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for user ID {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<string?> GetUserIdAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogDebug("No user ID found in ClaimsPrincipal");
                }
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from ClaimsPrincipal");
                return null;
            }
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            try
            {
                var identityUser = await _userManager.FindByIdAsync(userId);
                return identityUser != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists for ID {UserId}", userId);
                return false;
            }
        }
    }
}