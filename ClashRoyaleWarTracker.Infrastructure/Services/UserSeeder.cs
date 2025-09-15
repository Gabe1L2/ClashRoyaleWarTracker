using ClashRoyaleWarTracker.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClashRoyaleWarTracker.Infrastructure.Services
{
    public class UserSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<UserSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create roles if they don't exist - ordered from highest to lowest permission
                await CreateRoleIfNotExists(roleManager, "Admin");
                await CreateRoleIfNotExists(roleManager, "Management");
                await CreateRoleIfNotExists(roleManager, "Coleader");
                await CreateRoleIfNotExists(roleManager, "Member");
                await CreateRoleIfNotExists(roleManager, "Guest");

                // Read users from configuration
                var defaultUsers = _configuration.GetSection("DefaultUsers").Get<DefaultUser[]>();
                
                if (defaultUsers == null || defaultUsers.Length == 0)
                {
                    _logger.LogWarning("No default users configured in DefaultUsers section");
                    return;
                }

                foreach (var userData in defaultUsers)
                {
                    if (string.IsNullOrEmpty(userData.Username) || string.IsNullOrEmpty(userData.Password))
                    {
                        _logger.LogWarning("Skipping user with empty username or password");
                        continue;
                    }

                    try
                    {
                        var existingUser = await userManager.FindByNameAsync(userData.Username);
                        if (existingUser == null)
                        {
                            var user = new IdentityUser
                            {
                                UserName = userData.Username,
                                Email = null, // No email required
                                EmailConfirmed = false
                            };

                            var result = await userManager.CreateAsync(user, userData.Password);
                            if (result.Succeeded)
                            {
                                _logger.LogInformation("Created user: {Username}", userData.Username);
                                
                                // Assign role to user
                                var roleResult = await userManager.AddToRoleAsync(user, userData.Role);
                                if (roleResult.Succeeded)
                                {
                                    _logger.LogInformation("Assigned role {Role} to user {Username}", userData.Role, userData.Username);
                                }
                                else
                                {
                                    _logger.LogError("Failed to assign role {Role} to user {Username}: {Errors}", 
                                        userData.Role, userData.Username, 
                                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                                }
                            }
                            else
                            {
                                _logger.LogError("Failed to create user {Username}: {Errors}", 
                                    userData.Username, 
                                    string.Join(", ", result.Errors.Select(e => e.Description)));
                            }
                        }
                        else
                        {
                            _logger.LogInformation("User {Username} already exists, skipping creation", userData.Username);
                            
                            // Check if user has the correct role
                            if (!await userManager.IsInRoleAsync(existingUser, userData.Role))
                            {
                                // Remove from other roles first
                                var currentRoles = await userManager.GetRolesAsync(existingUser);
                                if (currentRoles.Any())
                                {
                                    await userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                                }
                                
                                // Add to correct role
                                await userManager.AddToRoleAsync(existingUser, userData.Role);
                                _logger.LogInformation("Updated role for user {Username} to {Role}", userData.Username, userData.Role);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing user {Username}: {Message}", userData.Username, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user seeding: {Message}", ex.Message);
            }
        }

        private async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleName, 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}