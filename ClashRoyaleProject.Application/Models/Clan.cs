using System.ComponentModel.DataAnnotations;

namespace ClashRoyaleProject.Application.Models
{
    public class Clan
    {
        public int ID { get; set; }
        [MaxLength(32)]
        public required string Tag { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }
        public int WarTrophies { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ClanHistory
    {
        public int ID { get; set; }
        public int ClanID { get; set; }
        public int SeasonID { get; set; }
        public int WeekIndex { get; set; }
        public int WarTrophies { get; set; }
        public DateTime RecordedDate { get; set; }
    }


}
