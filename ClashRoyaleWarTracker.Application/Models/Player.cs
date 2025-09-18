using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClashRoyaleWarTracker.Application.Models
{
    public class Player
    {
        public int ID { get; set; }
        public int? ClanID { get; set; }
        [MaxLength(32)]
        public required string Tag { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public DateTime LastUpdated { get; set; }
    }

    public class PlayerAverage
    {
        public int ID { get; set; }
        public required int PlayerID { get; set; }
        public int? ClanID { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal FameAttackAverage { get; set; }
        public int Attacks { get; set; }
        public bool Is5k { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PlayerAverageDTO
    {
        public int ID { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string PlayerTag { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int? ClanID { get; set; }
        public string? ClanName { get; set; }
        public string? ClanTag { get; set; }
        public decimal FameAttackAverage { get; set; }
        public int Attacks { get; set; }
        public bool Is5k { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}