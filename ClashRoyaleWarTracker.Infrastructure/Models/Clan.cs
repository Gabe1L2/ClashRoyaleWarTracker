using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashRoyaleWarTracker.Infrastructure.Models
{
    public class ClashRoyaleApiClan
    {
        public string? Tag { get; set; }
        public string? Name { get; set; }
        public int ClanWarTrophies { get; set; }
    }
}
