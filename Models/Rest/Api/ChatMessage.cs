using RomDiscord.Models.Db;
using System.Text.Json.Serialization;

namespace RomDiscord.Models.Rest.Api;


public class ItemInfo
{
    public class EnchantInfo
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Enchant Enchant { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Level { get; set; }
    }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RefineLevel { get; set; }
    public ulong Price { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Guid { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<EnchantInfo>? Enchants { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Broken { get; set; } = null;
    public int ItemId { get; set; }

}
public class NewChatMessage
{
    public enum MessageType
    {
        Guild,
        ChatBox
    }
    public string Message { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Guild;
    public List<ItemInfo> ItemInfos { get; set; } = new List<ItemInfo>();
    public ulong UserId { get; set; }
}


public class ChatMessage
{
    public string channel { get; set; }
    public string str { get; set; }
    public string name { get; set; }
    public string id { get; set; }
    public int portrait { get; set; }
    public string rolejob { get; set; }
    public int baselevel { get; set; }
    public int hair { get; set; }
    public int haircolor { get; set; }
    public int body { get; set; }
    public string gender { get; set; }
    public string guildname { get; set; }
    public int appellation { get; set; }
    public int head { get; set; }
    public int eye { get; set; }
    public string accid { get; set; }
    public int roomid { get; set; }
    public Photo photo { get; set; }
    public Expression expression { get; set; }
    public int serverid { get; set; }
}

public class Photo
{
}

public class Expression
{
}
