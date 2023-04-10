namespace RomDiscord.Models.Rest.Api;

public class MvpScan
{
    public int server { get; set; }
    public int zoneId { get; set; }
    public DateTime roundTime { get; set; } = DateTime.MinValue;
    public int id { get; set; }
    public string channel { get; set; } = string.Empty;
    public long refreshTime { get; set; }
    public string lastKiller { get; set; } = string.Empty;
    public int mapId { get; set; }
    public int level { get; set; }
    public long dieTime { get; set; }
    public long summonTime { get; set; }
    public bool special { get; set; } = false;
}
