namespace RomDiscord.Models.Rest.Api
{
	public class ExchangeScan
	{
        public class Entry
        {
            public long amount { get; set; }
            public ulong price { get; set; }
            public DateTime? snapTime { get; set; }
            public int refineLevel { get; set; }
            public bool broken { get; set; }
            public string? enchant1 { get; set; } = string.Empty;
            public string? enchant2 { get; set; } = string.Empty;
            public string? enchant3 { get; set; } = string.Empty;
            public string? enchant4 { get; set; } = string.Empty;
        }
        public DateTime scanTime { get; set; } = DateTime.MinValue;
        public int itemId { get; set; }
        public List<Entry> Entries { get; set; } = new List<Entry>();
    }
}
