using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClashRoyaleWarTracker.Application.Models
{
    public class Player
    {
        public int ID { get; set; }
        [MaxLength(32)]
        public required string Tag { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PlayerAverage
    {
        public int ID { get; set; }
        public required int PlayerID { get; set; }
        public required int ClanID { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal FameAttackAverage { get; set; }
        public bool Is5k { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}