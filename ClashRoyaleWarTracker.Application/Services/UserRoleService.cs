using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ClashRoyaleWarTracker.Application.Services
{
    public interface IUserRoleService
    {
        Task<UserRole> GetUserRoleAsync(ClaimsPrincipal user);
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission);
        Task<bool> HasRoleOrHigherAsync(ClaimsPrincipal user, UserRole minimumRole);
        Task<IList<string>> GetUserRolesAsync(ClaimsPrincipal user);
        Task<UserRole> GetHighestUserRoleAsync(ClaimsPrincipal user);
    }

    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserRoleService> _logger;
        private readonly IMemoryCache _cache;

        // Define role hierarchy with explicit ordering
        private static readonly Dictionary<string, UserRole> RoleMapping = new()
        {
            { "Admin", UserRole.Admin },
            { "Management", UserRole.Management },
            { "Coleader", UserRole.Coleader },
            { "Member", UserRole.Member },
            { "Guest", UserRole.Guest }
        };

        public UserRoleService(
            IUserRepository userRepository, 
            ILogger<UserRoleService> logger, 
            IMemoryCache cache)
        {
            _userRepository = userRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<UserRole> GetUserRoleAsync(ClaimsPrincipal user)
        {
            return await GetHighestUserRoleAsync(user);
        }

        public async Task<UserRole> GetHighestUserRoleAsync(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("User is not authenticated, returning Guest role");
                return UserRole.Guest;
            }

            var userId = await _userRepository.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("User ID is null or empty, returning Guest role");
                return UserRole.Guest;
            }

            // Try cache first
            var cacheKey = $"user_role_{userId}";
            if (_cache.TryGetValue(cacheKey, out UserRole cachedRole))
            {
                _logger.LogDebug("Retrieved cached role {Role} for user {UserId}", cachedRole, userId);
                return cachedRole;
            }

            try
            {
                var roles = await _userRepository.GetUserRolesAsync(user);
                
                if (roles == null || !roles.Any())
                {
                    _logger.LogDebug("No roles found for user ({UserId}), returning Guest role", userId);
                    return UserRole.Guest;
                }

                // Find the highest priority role (lowest enum value)
                var highestRole = UserRole.Guest; // Default to lowest privilege
                
                foreach (var role in roles)
                {
                    if (RoleMapping.TryGetValue(role, out var mappedRole))
                    {
                        if (mappedRole < highestRole) // Lower enum value = higher priority
                        {
                            highestRole = mappedRole;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Unknown role {Role} found for user ({UserId})", role, userId);
                    }
                }

                // Cache the result for 15 minutes
                _cache.Set(cacheKey, highestRole, TimeSpan.FromMinutes(15));
                
                _logger.LogDebug("Determined highest role {Role} for user ({UserId})", highestRole, userId);

                return highestRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role for user ({UserId})", userId);
                return UserRole.Guest;
            }
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
            {
                _logger.LogDebug("Permission is null or empty");
                return false;
            }

            var userRole = await GetUserRoleAsync(user);
            var hasPermission = RolePermissions.HasPermission(userRole, permission);
            
            _logger.LogDebug("User with role {Role} {HasPermission} permission {Permission}", 
                userRole, hasPermission ? "HAS" : "DOES NOT HAVE", permission);
            
            return hasPermission;
        }

        public async Task<bool> HasRoleOrHigherAsync(ClaimsPrincipal user, UserRole minimumRole)
        {
            var userRole = await GetUserRoleAsync(user);
            var hasRoleOrHigher = userRole <= minimumRole; // Lower enum values = higher permissions
            
            _logger.LogDebug("User with role {UserRole} {HasRole} minimum role {MinimumRole}", 
                userRole, hasRoleOrHigher ? "MEETS OR EXCEEDS" : "DOES NOT MEET", minimumRole);
            
            return hasRoleOrHigher;
        }

        public async Task<IList<string>> GetUserRolesAsync(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("User is not authenticated, returning empty role list");
                return new List<string>();
            }

            var userId = await _userRepository.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("User ID is null or empty, returning empty role list");
                return new List<string>();
            }

            // Try cache first
            var cacheKey = $"user_roles_{userId}";
            if (_cache.TryGetValue(cacheKey, out IList<string> cachedRoles))
            {
                _logger.LogDebug("Retrieved cached roles for user {UserId}: {Roles}", 
                    userId, string.Join(", ", cachedRoles));
                return cachedRoles;
            }

            try
            {
                // This now goes through the repository (Infrastructure layer)
                var roles = await _userRepository.GetUserRolesAsync(user);
                
                // Cache the result for 15 minutes
                _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(15));
                
                _logger.LogDebug("Retrieved roles for user ({UserId}): {Roles}", 
                    userId, string.Join(", ", roles));

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for user ({UserId})", userId);
                return new List<string>();
            }
        }
    }
}