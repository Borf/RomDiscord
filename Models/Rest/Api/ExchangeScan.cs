namespace RomDiscord.Models.Rest.Api
{
	public class ExchangeScan
	{
        public DateTime scanTime { get; set; } = DateTime.MinValue;
        public int itemId { get; set; }
        public long amount { get; set; }
        public long price { get; set; }
        public DateTime? snapTime { get; set; } = null;
        public int refineLevel { get; set; }
        public bool broken { get; set; }
        public string enchant1 { get; set; } = "";
        public string enchant2 { get; set; } = "";
        public string enchant3 { get; set; } = "";
        public string enchant4 { get; set; } = "";
        public string message { get; set; } = "";
    }
}
