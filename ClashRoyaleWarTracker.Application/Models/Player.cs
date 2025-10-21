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
        [MaxLength(100)]
        public string? Notes { get; set; }

        public DateTime LastUpdated { get; set; }
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
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

    public class RosterAssignment
    {
        public int ID { get; set; }
        public int SeasonID { get; set; }
        public int WeekIndex { get; set; }
        public int PlayerID { get; set; }
        public int? ClanID { get; set; }
        public bool IsInClan { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class RosterAssignmentDTO
    {
        public int ID { get; set; }
        public int SeasonID { get; set; }
        public int WeekIndex { get; set; }
        public int PlayerID { get; set; }
        public string PlayerTag { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal? FameAttackAverage { get; set; }
        public bool? Is5k { get; set; }
        public int? ClanID { get; set; }
        public string? ClanName { get; set; }
        public string? ClanTag { get; set; }
        public bool IsInClan { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class ClashRoyalePlayerInfo
    {
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CurrentClanTag { get; set; }
        public string? CurrentClanName { get; set; }
    }
}