using ClashRoyaleProject.Application;
using ClashRoyaleProject.Infrastructure;
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
                try
                {
                    await dbContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    throw;
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