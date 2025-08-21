using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Infrastructure;
using ClashRoyaleProject.Infrastructure.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClashRoyaleProject.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // --- SECTION: Configure Clash Royale API Client ---

            // 1. Get the API key from your appsettings.json file.
            string apiKey = builder.Configuration["ClashRoyaleApi:ApiKey"]
                ?? throw new InvalidOperationException("Clash Royale API Key not found in configuration.");

            // 2. Register a typed HttpClient for our Clash Royale API client.
            builder.Services.AddHttpClient<IClashRoyaleApiClient, ClashRoyaleApiClient>(client =>
            {
                // Set the base address for all requests made by this client.
                client.BaseAddress = new Uri(builder.Configuration["ClashRoyaleApi:BaseUrl"]);

                // Add the Authorization header with the Bearer token for every request.
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            });

            // --- END SECTION ---

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
