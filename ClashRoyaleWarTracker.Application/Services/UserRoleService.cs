using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ClashRoyaleWarTracker.Application.Services
{
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

        public UserRoleService(IUserRepository userRepository, ILogger<UserRoleService> logger, IMemoryCache cache)
        {
            _userRepository = userRepository;
            _logger = logger;
            _cache = cache;
        }

        #region Role Permission Logic

        public async Task<ServiceResult<UserRole>> GetUserRoleAsync(ClaimsPrincipal user)
        {
            return await GetHighestUserRoleAsync(user);
        }

        public async Task<ServiceResult<UserRole>> GetHighestUserRoleAsync(ClaimsPrincipal user)
        {
            try
            {
                if (user?.Identity?.IsAuthenticated != true)
                {
                    _logger.LogDebug("User is not authenticated, returning Guest role");
                    return ServiceResult<UserRole>.Successful(UserRole.Guest, "User is not authenticated");
                }

                var userId = _userRepository.GetUserIdAsync(user);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogDebug("User ID is null or empty, returning Guest role");
                    return ServiceResult<UserRole>.Successful(UserRole.Guest, "User ID not found");
                }

                // Try cache first
                var cacheKey = $"user_role_{userId}";
                if (_cache.TryGetValue(cacheKey, out UserRole cachedRole))
                {
                    _logger.LogDebug("Retrieved cached role {Role} for user {UserId}", cachedRole, userId);
                    return ServiceResult<UserRole>.Successful(cachedRole, "Retrieved from cache");
                }

                var roles = await _userRepository.GetUserRolesAsync(user);
                
                if (roles == null || !roles.Any())
                {
                    _logger.LogDebug("No roles found for user ({UserId}), returning Guest role", userId);
                    return ServiceResult<UserRole>.Successful(UserRole.Guest, "No roles assigned to user");
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

                return ServiceResult<UserRole>.Successful(highestRole, $"User role determined: {highestRole}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role for user ({UserId})", user?.Identity?.Name);
                return ServiceResult<UserRole>.Failure("An error occurred while determining user role");
            }
        }

        public async Task<ServiceResult<bool>> HasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            try
            {
                UserRole userRole;
                if (string.IsNullOrWhiteSpace(permission))
                {
                    _logger.LogDebug("Permission is null or empty");
                    return ServiceResult<bool>.Successful(false, "Permission parameter is required");
                }

                // Get current user's role
                var getUserRoleResult = await GetUserRoleAsync(user);
                if (getUserRoleResult.Success)
                {
                    userRole = getUserRoleResult.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to get user role, defaulting to Guest: {Message}", getUserRoleResult.Message);
                    userRole = UserRole.Guest;
                }

                var hasPermission = RolePermissions.HasPermission(userRole, permission);
                
                _logger.LogDebug("User with role {Role} {HasPermission} permission {Permission}",
                    userRole, hasPermission ? "HAS" : "DOES NOT HAVE", permission);
                
                var message = hasPermission 
                    ? $"User has {permission} permission" 
                    : $"User does not have {permission} permission";

                return ServiceResult<bool>.Successful(hasPermission, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user", permission);
                return ServiceResult<bool>.Failure("An error occurred while checking user permissions");
            }
        }

        public async Task<ServiceResult<bool>> HasRoleOrHigherAsync(ClaimsPrincipal user, UserRole minimumRole)
        {
            try
            {
                var userRoleResult = await GetUserRoleAsync(user);
                if (!userRoleResult.Success)
                {
                    _logger.LogWarning("Failed to get user role for role comparison: {Error}", userRoleResult.Message);
                    return ServiceResult<bool>.Failure($"Unable to determine user role: {userRoleResult.Message}");
                }

                var hasRoleOrHigher = userRoleResult.Data <= minimumRole; // Lower enum values = higher permissions
                
                _logger.LogDebug("User with role {UserRole} {HasRole} minimum role {MinimumRole}", 
                    userRoleResult.Data, hasRoleOrHigher ? "MEETS OR EXCEEDS" : "DOES NOT MEET", minimumRole);
                
                var message = hasRoleOrHigher 
                    ? $"User role {userRoleResult.Data} meets or exceeds minimum role {minimumRole}" 
                    : $"User role {userRoleResult.Data} does not meet minimum role {minimumRole}";

                return ServiceResult<bool>.Successful(hasRoleOrHigher, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role hierarchy for minimum role {MinimumRole}", minimumRole);
                return ServiceResult<bool>.Failure("An error occurred while checking user role hierarchy");
            }
        }

        public async Task<ServiceResult<IList<string>>> GetUserRolesAsync(ClaimsPrincipal user)
        {
            try
            {
                if (user?.Identity?.IsAuthenticated != true)
                {
                    _logger.LogDebug("User is not authenticated, returning empty role list");
                    return ServiceResult<IList<string>>.Successful(new List<string>(), "User is not authenticated");
                }

                var userId = _userRepository.GetUserIdAsync(user);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogDebug("User ID is null or empty, returning empty role list");
                    return ServiceResult<IList<string>>.Successful(new List<string>(), "User ID not found");
                }

                // Try cache first
                var cacheKey = $"user_roles_{userId}";
                if (_cache.TryGetValue(cacheKey, out IList<string> cachedRoles))
                {
                    _logger.LogDebug("Retrieved cached roles for user {UserId}: {Roles}", 
                        userId, string.Join(", ", cachedRoles));
                    return ServiceResult<IList<string>>.Successful(cachedRoles, "Retrieved roles from cache");
                }

                var roles = await _userRepository.GetUserRolesAsync(user);
                
                // Cache the result for 15 minutes
                _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(15));
                
                _logger.LogDebug("Retrieved roles for user ({UserId}): {Roles}", 
                    userId, string.Join(", ", roles));

                return ServiceResult<IList<string>>.Successful(roles, $"Retrieved {roles.Count} roles for user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for user ({UserId})", user?.Identity?.Name);
                return ServiceResult<IList<string>>.Failure("An error occurred while retrieving user roles");
            }
        }

        #endregion

        #region User Management Methods

        public async Task<ServiceResult<IList<UserWithRoles>>> GetAllUsersWithRolesAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all users with roles through UserRoleService");
                
                var users = await _userRepository.GetAllUsersWithRolesAsync();
                
                _logger.LogDebug("Successfully retrieved {UserCount} users with roles", users.Count);
                return ServiceResult<IList<UserWithRoles>>.Successful(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users with roles");
                return ServiceResult<IList<UserWithRoles>>.Failure("Failed to retrieve users");
            }
        }

        public async Task<ServiceResult<IList<string>>> GetAllRolesAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all available roles through UserRoleService");
                
                var roles = await _userRepository.GetAllRolesAsync();
                
                _logger.LogDebug("Successfully retrieved {RoleCount} roles", roles.Count);
                return ServiceResult<IList<string>>.Successful(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available roles");
                return ServiceResult<IList<string>>.Failure("Failed to retrieve roles");
            }
        }

        public async Task<ServiceResult> CreateUserAsync(string username, string password, string role)
        {
            try
            {
                _logger.LogDebug("Creating user {Username} with role {Role} through UserRoleService", username, role);

                // Input validation
                if (string.IsNullOrWhiteSpace(username))
                {
                    return ServiceResult.Failure("Username is required");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return ServiceResult.Failure("Password is required");
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    return ServiceResult.Failure("Role is required");
                }

                // Business rule: Only allow valid roles
                if (!RoleMapping.ContainsKey(role))
                {
                    return ServiceResult.Failure($"Invalid role '{role}' specified");
                }

                var result = await _userRepository.CreateUserAsync(username, password, role);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully created user {Username} with role {Role}", username, role);
                    
                    // Clear any cached user data since we added a new user
                    InvalidateUserCaches();
                    
                    return ServiceResult.Successful($"User '{username}' created successfully with role '{role}'");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to create user {Username}: {Errors}", username, errors);
                    return ServiceResult.Failure($"Failed to create user: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", username);
                return ServiceResult.Failure("An unexpected error occurred while creating the user");
            }
        }

        public async Task<ServiceResult> DeleteUserAsync(string userId)
        {
            try
            {
                _logger.LogDebug("Deleting user {UserId} through UserRoleService", userId);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult.Failure("User ID is required");
                }

                var result = await _userRepository.DeleteUserAsync(userId);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully deleted user {UserId}", userId);
                    
                    // Clear any cached user data since we deleted a user
                    InvalidateUserCaches();
                    
                    return ServiceResult.Successful("User deleted successfully");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to delete user {UserId}: {Errors}", userId, errors);
                    return ServiceResult.Failure($"Failed to delete user: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return ServiceResult.Failure("An unexpected error occurred while deleting the user");
            }
        }

        public async Task<ServiceResult> UpdateUserRoleAsync(string userId, string newRole)
        {
            try
            {
                _logger.LogDebug("Updating user {UserId} role to {Role} through UserRoleService", userId, newRole);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult.Failure("User ID is required");
                }

                if (string.IsNullOrWhiteSpace(newRole))
                {
                    return ServiceResult.Failure("Role is required");
                }

                // Business rule: Only allow valid roles
                if (!RoleMapping.ContainsKey(newRole))
                {
                    return ServiceResult.Failure($"Invalid role '{newRole}' specified");
                }

                var result = await _userRepository.UpdateUserRoleAsync(userId, newRole);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully updated user {UserId} role to {Role}", userId, newRole);
                    
                    // Clear cached role data for this user
                    InvalidateUserCaches();
                    
                    return ServiceResult.Successful($"User role updated to '{newRole}' successfully");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to update user {UserId} role: {Errors}", userId, errors);
                    return ServiceResult.Failure($"Failed to update user role: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} role to {Role}", userId, newRole);
                return ServiceResult.Failure("An unexpected error occurred while updating the user role");
            }
        }

        public async Task<ServiceResult> ChangePasswordAsync(string userId, string newPassword)
        {
            try
            {
                _logger.LogDebug("Changing password for user {UserId} through UserRoleService", userId);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult.Failure("User ID is required");
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    return ServiceResult.Failure("Password is required");
                }

                if (newPassword.Length < 6)
                {
                    return ServiceResult.Failure("Password must be at least 6 characters long");
                }

                var result = await _userRepository.ChangePasswordAsync(userId, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully changed password for user {UserId}", userId);
                    
                    // Clear cached data since user was updated
                    InvalidateUserCaches();
                    
                    return ServiceResult.Successful("Password changed successfully");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to change password for user {UserId}: {Errors}", userId, errors);
                    return ServiceResult.Failure($"Failed to change password: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return ServiceResult.Failure("An unexpected error occurred while changing the password");
            }
        }

        #endregion

        #region Cache Management

        private void InvalidateUserCaches()
        {
            // Since we don't have a way to enumerate cache keys, we rely on TTL
            // This could be enhanced with a more sophisticated cache invalidation strategy
            _logger.LogDebug("User cache invalidation requested - relying on TTL expiration");
        }

        #endregion
    }
}