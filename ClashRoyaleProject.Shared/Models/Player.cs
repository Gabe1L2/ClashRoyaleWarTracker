namespace ClashRoyaleProject.Shared.Models
{
    public class Player
    {
        public int ID { get; set; }
        public required string Tag { get; set; }
        public required string Name { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class Clan
    {
        public int ID { get; set; }
        public required string Tag { get; set; }
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

    public class RawWarData
    {
        public int ID { get; set; }
        public int PlayerID { get; set; }
        public int ClanHistoryID { get; set; }
        public int Fame { get; set; }
        public int RepairPoints { get; set; }
        public int BoatAttacks { get; set; }
        public int DecksUsed { get; set; }
        public DateTime InsertDate { get; set; }
    }

    public class WarData
    {
        public int ID { get; set; }
        public int PlayerID { get; set; }
        public int ClanHistoryID { get; set; }
        public int Fame { get; set; }
        public int DecksUsed { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PlayerAverage
    {
        public int ID { get; set; }
        public int PlayerID { get; set; }
        public int ClanID { get; set; }
        public double FameAttackAverage { get; set; }
        public bool Is5k { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}