using ClashRoyaleProject.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClashRoyaleProject.Infrastructure.Services
{
    public class UserSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider; // to create scope and get UserManager
        private readonly IConfiguration _configuration; // reads from appsettings.json and secrets
        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<UserSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope(); // create scope to get scoped services
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(); // get UserManager

            // Read users from configuration
            var defaultUsers = _configuration.GetSection("DefaultUsers").Get<DefaultUser[]>();
            
            if (defaultUsers == null || defaultUsers.Length == 0)
            {
                _logger.LogWarning("No default users configured in DefaultUsers section");
                return;
            }

            foreach (var userData in defaultUsers)
            {
                if (string.IsNullOrEmpty(userData.Email) || string.IsNullOrEmpty(userData.Password))
                {
                    _logger.LogWarning("Skipping user with empty email or password");
                    continue;
                }

                try
                {
                    var existingUser = await userManager.FindByEmailAsync(userData.Email);
                    if (existingUser == null)
                    {
                        var user = new IdentityUser
                        {
                            UserName = userData.Email,
                            Email = userData.Email,
                            EmailConfirmed = true // Auto-confirm since you control access
                        };

                        var result = await userManager.CreateAsync(user, userData.Password);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("Created user: {Email}", userData.Email);
                        }
                        else
                        {
                            _logger.LogError("Failed to create user {Email}: {Errors}", 
                                userData.Email, 
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        _logger.LogInformation("User {Email} already exists, skipping creation", userData.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing user {Email}", userData.Email);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}