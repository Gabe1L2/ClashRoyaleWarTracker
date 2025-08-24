using ClashRoyaleProject.Application;
using ClashRoyaleProject.Infrastructure;

namespace ClashRoyaleProject.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add layers following clean architecture
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            
            // Add Razor Pages
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseExceptionHandler("/Error");
            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Enable authentication
            app.UseAuthorization();  // Enable authorization

            app.MapRazorPages();

            app.Run();
        }
    }
}
