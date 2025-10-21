using ClashRoyaleWarTracker.Application;
using ClashRoyaleWarTracker.Infrastructure;
using ClashRoyaleWarTracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ClashRoyaleWarTracker.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add layers following clean architecture
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            
            // Add Razor Pages
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Ensure database is created and migrations are applied at startup
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    logger.LogInformation("Applying database migrations...");
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Database migrations completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    throw;
                }

                try
                {
                    logger.LogInformation("Starting user seeding...");
                    // Run user seeding after migrations are complete
                    var userSeeder = new UserSeeder(
                        app.Services, 
                        app.Configuration, 
                        scope.ServiceProvider.GetRequiredService<ILogger<UserSeeder>>());
                    await userSeeder.StartAsync(CancellationToken.None);
                    logger.LogInformation("User seeding completed.");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "User seeding failed, but application will continue. Users may need to be created manually.");
                }
            }

            // Configure the HTTP request pipeline.
            app.UseExceptionHandler("/Error");
            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Enable authentication
            app.UseAuthorization();  // Enable authorization

            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower();

                // Block all Identity/Account/Manage pages except ChangePassword
                if (path?.StartsWith("/identity/account/manage") == true &&
                    !path.Contains("/changepassword"))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                // Block Guest user from accessing password change page
                if (path?.Contains("/changepassword") == true &&
                    context.User?.Identity?.IsAuthenticated == true &&
                    context.User?.Identity?.Name == "Guest")
                {
                    context.Response.StatusCode = 403; // Forbidden
                    await context.Response.WriteAsync("Guest account cannot change their password.");
                    return;
                }

                await next();
            });

            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}