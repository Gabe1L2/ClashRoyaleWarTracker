using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Services;
using ClashRoyaleWarTracker.Infrastructure.Http;
using ClashRoyaleWarTracker.Infrastructure.Repositories;
using ClashRoyaleWarTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace ClashRoyaleWarTracker.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Identity configuration with controlled access
            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                // Disable email confirmation requirement since you'll manage users manually
                options.SignIn.RequireConfirmedAccount = false;

                // Make passwords simple since you control who gets access
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<IClashRoyaleService, ClashRoyaleService>();
            services.AddScoped<IClanRepository, ClanRepository>();
            services.AddScoped<IApplicationService, ApplicationService>();

            // Clash Royale API configuration
            var apiKey = configuration["ClashRoyaleApi:ApiKey"]
                ?? throw new InvalidOperationException("Clash Royale API Key not found in configuration.");

            var baseUrl = configuration["ClashRoyaleApi:BaseUrl"]
                ?? throw new InvalidOperationException("Clash Royale API Base URL not found in configuration.");

            // Register HttpClient with proper headers configuration
            services.AddHttpClient<IClashRoyaleApiClient, ClashRoyaleApiClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            return services;
        }
    }
}