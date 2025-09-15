using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Models;
using ClashRoyaleWarTracker.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClashRoyaleWarTracker.Web.Pages
{
    [Authorize]
    public class UserManagementModel : PageModel
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<UserManagementModel> _logger;

        public UserManagementModel(
            IUserRoleService userRoleService,
            ILogger<UserManagementModel> logger)
        {
            _userRoleService = userRoleService;
            _logger = logger;
        }

        public IList<UserWithRoles> Users { get; set; } = new List<UserWithRoles>();
        public IList<string> AvailableRoles { get; set; } = new List<string>();
        public UserRole CurrentUserRole { get; set; } = UserRole.Guest;
        public bool CanManageUsers => RolePermissions.HasPermission(CurrentUserRole, Permissions.ManageUsers);

        [BindProperty]
        public CreateUserInputModel CreateUserInput { get; set; } = new();

        public class CreateUserInputModel
        {
            [StringLength(50, MinimumLength = 3)]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Role")]
            public string Role { get; set; } = string.Empty;
        }

        [BindProperty]
        public EditUserInputModel EditUserInput { get; set; } = new();

        public class EditUserInputModel
        {
            public string UserId { get; set; } = string.Empty;

            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string? NewPassword { get; set; }

            [Display(Name = "Role")]
            public string Role { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current user's role and check permissions
            var userRoleResult = await _userRoleService.GetUserRoleAsync(User);
            if (userRoleResult.Success)
            {
                CurrentUserRole = userRoleResult.Data;
            }
            else
            {
                _logger.LogWarning("Failed to get user role: {Message}", userRoleResult.Message);
                CurrentUserRole = UserRole.Guest;
            }

            if (!CanManageUsers)
            {
                _logger.LogWarning("User {UserName} attempted to access User Management without proper permissions", User.Identity?.Name);
                return Forbid();
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateUserAsync()
        {
            // Get current user's role and check permissions
            var userRoleResult = await _userRoleService.GetUserRoleAsync(User);
            if (userRoleResult.Success)
            {
                CurrentUserRole = userRoleResult.Data;
            }
            else
            {
                _logger.LogWarning("Failed to get user role: {Message}", userRoleResult.Message);
                CurrentUserRole = UserRole.Guest;
            }

            if (!CanManageUsers)
            {
                TempData["ErrorMessage"] = "You don't have permission to manage users.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var result = await _userRoleService.CreateUserAsync(CreateUserInput.Username, CreateUserInput.Password, CreateUserInput.Role);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    _logger.LogInformation("User {UserName} created user {NewUserName} with role {Role}", 
                        User.Identity?.Name, CreateUserInput.Username, CreateUserInput.Role);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    _logger.LogWarning("Failed to create user {UserName}: {Error}", 
                        CreateUserInput.Username, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {UserName}", CreateUserInput.Username);
                TempData["ErrorMessage"] = "An unexpected error occurred while creating the user.";
            }

            // Clear the form
            CreateUserInput = new CreateUserInputModel();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            // Get current user's role and check permissions
            var userRoleResult = await _userRoleService.GetUserRoleAsync(User);
            if (userRoleResult.Success)
            {
                CurrentUserRole = userRoleResult.Data;
            }
            else
            {
                _logger.LogWarning("Failed to get user role: {Message}", userRoleResult.Message);
                CurrentUserRole = UserRole.Guest;
            }

            if (!CanManageUsers)
            {
                TempData["ErrorMessage"] = "You don't have permission to manage users.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToPage();
            }

            try
            {
                // Now going through the UserRoleService (Application layer)
                var result = await _userRoleService.DeleteUserAsync(userId);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    _logger.LogInformation("User {UserName} deleted user with ID {UserId}", 
                        User.Identity?.Name, userId);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    _logger.LogWarning("Failed to delete user {UserId}: {Error}", userId, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the user.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync()
        {
            // Get current user's role and check permissions
            var userRoleResult = await _userRoleService.GetUserRoleAsync(User);
            if (userRoleResult.Success)
            {
                CurrentUserRole = userRoleResult.Data;
            }
            else
            {
                _logger.LogWarning("Failed to get user role: {Message}", userRoleResult.Message);
                CurrentUserRole = UserRole.Guest;
            }

            if (!CanManageUsers)
            {
                TempData["ErrorMessage"] = "You don't have permission to manage users.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(EditUserInput.UserId) || string.IsNullOrWhiteSpace(EditUserInput.Role))
            {
                TempData["ErrorMessage"] = "User ID and Role are required.";
                return RedirectToPage();
            }

            try
            {
                var roleResult = await _userRoleService.UpdateUserRoleAsync(EditUserInput.UserId, EditUserInput.Role);
                if (!roleResult.Success)
                {
                    TempData["ErrorMessage"] = roleResult.Message;
                    return RedirectToPage();
                }

                string successMessage = roleResult.Message;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(EditUserInput.NewPassword))
                {
                    var passwordResult = await _userRoleService.ChangePasswordAsync(EditUserInput.UserId, EditUserInput.NewPassword);
                    if (!passwordResult.Success)
                    {
                        TempData["ErrorMessage"] = $"Role updated successfully, but password change failed: {passwordResult.Message}";
                        return RedirectToPage();
                    }
                    successMessage += " and password updated";
                }

                TempData["SuccessMessage"] = successMessage + " successfully.";
                _logger.LogInformation("User {CurrentUser} updated user {UserId} - role: {Role}, password: {PasswordChanged}", 
                    User.Identity?.Name, EditUserInput.UserId, EditUserInput.Role, !string.IsNullOrWhiteSpace(EditUserInput.NewPassword));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", EditUserInput.UserId);
                TempData["ErrorMessage"] = "An unexpected error occurred while updating the user.";
            }

            // Clear the form
            EditUserInput = new EditUserInputModel();
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var usersResult = await _userRoleService.GetAllUsersWithRolesAsync();
                if (usersResult.Success && usersResult.Data != null)
                {
                    Users = usersResult.Data;
                }
                else
                {
                    TempData["ErrorMessage"] = usersResult.Message;
                    Users = new List<UserWithRoles>();
                }

                var rolesResult = await _userRoleService.GetAllRolesAsync();
                if (rolesResult.Success && rolesResult.Data != null)
                {
                    AvailableRoles = rolesResult.Data;
                }
                else
                {
                    TempData["ErrorMessage"] = rolesResult.Message;
                    AvailableRoles = new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management data");
                Users = new List<UserWithRoles>();
                AvailableRoles = new List<string>();
                TempData["ErrorMessage"] = "Error loading user data.";
            }
        }
    }
}