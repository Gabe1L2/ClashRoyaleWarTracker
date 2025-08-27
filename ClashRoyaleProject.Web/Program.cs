using ClashRoyaleProject.Application;
using ClashRoyaleProject.Infrastructure;
using ClashRoyaleProject.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ClashRoyaleProject.Web
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
                    // Don't throw - let the application start even if seeding fails
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

            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}