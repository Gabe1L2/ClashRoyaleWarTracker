namespace ClashRoyaleProject.Shared.Models
{
    public class Player
    {
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TrophyCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class Clan
    {
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}