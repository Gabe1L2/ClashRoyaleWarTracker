using Microsoft.Extensions.DependencyInjection;

namespace ClashRoyaleWarTracker.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services here
            // Example: services.AddScoped<IPlayerService, PlayerService>();
            
            return services;
        }
    }
}