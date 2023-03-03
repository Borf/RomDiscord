using RomDiscord.Models.Db;
using System.Text.Json.Serialization;

namespace RomDiscord.Models.Rest.Api;

public class NewExchangeScan
{
    public class EnchantInfo
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Enchant Enchant { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Level { get; set; }
    }

    public class ItemData
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? RefineLevel { get; set; }
        public ulong Price { get; set; }
        public ulong Amount { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ulong EndTime { get; set; } = 0;
        public string Guid { get; set; } = "";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<EnchantInfo>? Enchants { get; set; }
    }

    public DateTime ScanTime { get; set; }
    public int ItemId { get; set; }
    public List<ItemData> Data { get; set; } = new List<ItemData>();
}