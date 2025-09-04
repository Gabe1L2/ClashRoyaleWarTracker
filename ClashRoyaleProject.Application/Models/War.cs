using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClashRoyaleProject.Application.Models
{

    [Table("RawWarData")]
    public class RawWarHistory
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

    [Table("WarData")]
    public class WarHistory
    {
        public int ID { get; set; }
        public required int PlayerID { get; set; }
        public required int ClanHistoryID { get; set; }
        public int Fame { get; set; }
        public int DecksUsed { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class RiverRaceLogResponse
    {
        [JsonPropertyName("items")]
        public List<RiverRaceItem> Items { get; set; } = new();
    }

    public class RiverRaceItem
    {
        [JsonPropertyName("seasonId")]
        public int SeasonId { get; set; }

        [JsonPropertyName("sectionIndex")]
        public int SectionIndex { get; set; }

        [JsonPropertyName("createdDate")]
        public string CreatedDate { get; set; } = string.Empty;

        [JsonPropertyName("standings")]
        public List<Standing> Standings { get; set; } = new();
    }

    public class Standing
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("trophyChange")]
        public int TrophyChange { get; set; }

        [JsonPropertyName("clan")]
        public ClanStanding Clan { get; set; } = new();
    }

    public class ClanStanding
    {
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("badgeId")]
        public int BadgeId { get; set; }

        [JsonPropertyName("fame")]
        public int Fame { get; set; }

        [JsonPropertyName("repairPoints")]
        public int RepairPoints { get; set; }

        [JsonPropertyName("finishTime")]
        public string FinishTime { get; set; } = string.Empty;

        [JsonPropertyName("participants")]
        public List<Participant> Participants { get; set; } = new();

        [JsonPropertyName("periodPoints")]
        public int PeriodPoints { get; set; }

        [JsonPropertyName("clanScore")]
        public int ClanScore { get; set; }
    }

    public class Participant
    {
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("fame")]
        public int Fame { get; set; }

        [JsonPropertyName("repairPoints")]
        public int RepairPoints { get; set; }

        [JsonPropertyName("boatAttacks")]
        public int BoatAttacks { get; set; }

        [JsonPropertyName("decksUsed")]
        public int DecksUsed { get; set; }

        [JsonPropertyName("decksUsedToday")]
        public int DecksUsedToday { get; set; }
    }
}