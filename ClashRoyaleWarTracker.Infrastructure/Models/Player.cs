using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClashRoyaleWarTracker.Infrastructure.Models
{
    public class ClashRoyaleApiPlayer
    {
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("expLevel")]
        public int ExpLevel { get; set; }

        [JsonPropertyName("trophies")]
        public int Trophies { get; set; }

        [JsonPropertyName("clan")]
        public ClashRoyaleApiPlayerClan? Clan { get; set; }
    }
}
