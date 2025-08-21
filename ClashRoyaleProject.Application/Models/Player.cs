namespace ClashRoyaleProject.Application.Models
{
    public class Player
    {
        public int ID { get; set; }
        public required string Tag { get; set; }
        public required string Name { get; set; }
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