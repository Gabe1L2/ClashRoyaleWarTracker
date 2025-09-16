using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashRoyaleWarTracker.Application.Interfaces
{
    public interface ITimeZoneService
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
        DateTime ConvertUtcToCentral(DateTime utcTime);
        DateTime ConvertCentralToUtc(DateTime centralTime);
        string GetTimeZoneDisplayName();
    }
}
