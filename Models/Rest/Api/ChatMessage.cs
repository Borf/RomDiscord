namespace RomDiscord.Models.Rest.Api;


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
