using ClashRoyaleWarTracker.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserRepository> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

        public string? GetUserIdAsync(ClaimsPrincipal user)
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
        public async Task<IList<UserWithRoles>> GetAllUsersWithRolesAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all users with their roles");

                var users = await _userManager.Users.ToListAsync();
                var usersWithRoles = new List<UserWithRoles>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "No Role";

                    usersWithRoles.Add(new UserWithRoles
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "Unknown",
                        Role = primaryRole,
                    });
                }

                _logger.LogDebug("Retrieved {UserCount} users with roles", usersWithRoles.Count);
                return usersWithRoles.OrderBy(u => u.UserName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users with roles");
                return new List<UserWithRoles>();
            }
        }

        public async Task<IList<string>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                _logger.LogDebug("Retrieved {RoleCount} roles", roles.Count);
                return roles.Where(r => !string.IsNullOrEmpty(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                return new List<string>();
            }
        }

        public async Task<IdentityResult> CreateUserAsync(string username, string password, string role)
        {
            try
            {
                _logger.LogDebug("Creating user {UserName} with role {Role}", username, role);

                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    _logger.LogWarning("User {UserName} already exists", username);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicateUserName",
                        Description = $"Username '{username}' is already taken."
                    });
                }

                // Check if role exists
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    _logger.LogWarning("Role {Role} does not exist", role);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "InvalidRole",
                        Description = $"Role '{role}' does not exist."
                    });
                }

                // Create the user
                var user = new IdentityUser
                {
                    UserName = username,
                    Email = null, // No email required in your system
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Failed to create user {UserName}: {Errors}",
                        username, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return createResult;
                }

                // Assign role to user
                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to assign role {Role} to user {UserName}: {Errors}",
                        role, username, string.Join(", ", roleResult.Errors.Select(e => e.Description)));

                    // Clean up: delete the user if role assignment failed
                    await _userManager.DeleteAsync(user);
                    return roleResult;
                }

                _logger.LogInformation("Successfully created user {UserName} with role {Role}", username, role);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {UserName}", username);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UnexpectedError",
                    Description = "An unexpected error occurred while creating the user."
                });
            }
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            try
            {
                _logger.LogDebug("Deleting user with ID {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "User not found."
                    });
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully deleted user {UserName} ({UserId})", user.UserName, userId);
                }
                else
                {
                    _logger.LogError("Failed to delete user {UserName} ({UserId}): {Errors}",
                        user.UserName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", userId);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UnexpectedError",
                    Description = "An unexpected error occurred while deleting the user."
                });
            }
        }

        public async Task<IdentityResult> UpdateUserRoleAsync(string userId, string newRole)
        {
            try
            {
                _logger.LogDebug("Updating user {UserId} to role {Role}", userId, newRole);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "User not found."
                    });
                }

                // Check if role exists
                if (!await _roleManager.RoleExistsAsync(newRole))
                {
                    _logger.LogWarning("Role {Role} does not exist", newRole);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "InvalidRole",
                        Description = $"Role '{newRole}' does not exist."
                    });
                }

                // Remove from all current roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove current roles from user {UserName}: {Errors}",
                            user.UserName, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        return removeResult;
                    }
                }

                // Add to new role
                var addResult = await _userManager.AddToRoleAsync(user, newRole);
                if (addResult.Succeeded)
                {
                    _logger.LogInformation("Successfully updated user {UserName} to role {Role}", user.UserName, newRole);
                }
                else
                {
                    _logger.LogError("Failed to add user {UserName} to role {Role}: {Errors}",
                        user.UserName, newRole, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                }

                return addResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} role to {Role}", userId, newRole);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UnexpectedError",
                    Description = "An unexpected error occurred while updating the user role."
                });
            }
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string newPassword)
        {
            try
            {
                _logger.LogDebug("Changing password for user {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "User not found."
                    });
                }

                // Remove current password and set new one
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully changed password for user {UserName} ({UserId})", user.UserName, userId);
                }
                else
                {
                    _logger.LogError("Failed to change password for user {UserName} ({UserId}): {Errors}",
                        user.UserName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UnexpectedError",
                    Description = "An unexpected error occurred while changing the password."
                });
            }
        }
    }
}