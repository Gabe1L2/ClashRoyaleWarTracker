namespace ClashRoyaleProject.Application.Models
{
    public class RawWarData
    {
        public int ID { get; set; }
        public required int PlayerID { get; set; }
        public required int ClanHistoryID { get; set; }
        public int Fame { get; set; }
        public int RepairPoints { get; set; }
        public int BoatAttacks { get; set; }
        public int DecksUsed { get; set; }
        public DateTime InsertDate { get; set; }
    }

    public class WarData
    {
        public int ID { get; set; }
        public required int PlayerID { get; set; }
        public required int ClanHistoryID { get; set; }
        public int Fame { get; set; }
        public int DecksUsed { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}