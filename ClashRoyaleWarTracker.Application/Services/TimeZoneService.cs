using Microsoft.Extensions.Logging;
using ClashRoyaleWarTracker.Application.Interfaces;

namespace ClashRoyaleWarTracker.Application.Services
{

    public class CentralTimeZoneService : ITimeZoneService
    {
        private static readonly TimeZoneInfo CentralTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        private readonly ILogger<CentralTimeZoneService> _logger;

        public CentralTimeZoneService(ILogger<CentralTimeZoneService> logger)
        {
            _logger = logger;
        }

        // Always returns Central Time
        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CentralTimeZone);
        
        // Returns UTC for when you need it
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime ConvertUtcToCentral(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, CentralTimeZone);
        }

        public DateTime ConvertCentralToUtc(DateTime centralTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);
        }

        public string GetTimeZoneDisplayName()
        {
            return CentralTimeZone.DisplayName;
        }
    }
}