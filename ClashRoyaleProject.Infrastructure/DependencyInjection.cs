using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Services;
using ClashRoyaleProject.Infrastructure.Http;
using ClashRoyaleProject.Infrastructure.Repositories;
using ClashRoyaleProject.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClashRoyaleProject.Infrastructure
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

            services.AddHttpClient<IClashRoyaleApiClient, ClashRoyaleApiClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            });

            return services;
        }
    }
}