namespace RomDiscord.Models.Rest.Api;


public class FriendFindData
{
    public Data[] datas { get; set; }
}

public class Data
{
    public string guid { get; set; }
    public string accid { get; set; }
    public int level { get; set; }
    public int offlinetime { get; set; }
    public int hair { get; set; }
    public int haircolor { get; set; }
    public int body { get; set; }
    public int head { get; set; }
    public int face { get; set; }
    public int mouth { get; set; }
    public int eye { get; set; }
//    public string profession { get; set; }
    public string gender { get; set; }
    public string name { get; set; }
    public string guildname { get; set; }
    public string guildportrait { get; set; }
    public int roomid { get; set; }
    public int portraitFrame { get; set; }
    public int serverid { get; set; }
}

