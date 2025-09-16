using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClashRoyaleWarTracker.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Time Zone Service
            services.AddSingleton<ITimeZoneService, CentralTimeZoneService>();

            // Business/Domain Services
            services.AddScoped<IClashRoyaleService, ClashRoyaleService>();
            services.AddScoped<IApplicationService, ApplicationService>();

            // User/Auth Services  
            services.AddScoped<IUserRoleService, UserRoleService>();

            return services;
        }
    }
}