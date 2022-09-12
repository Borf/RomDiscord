using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class ExchangeScan
	{
		[Key]
		public int ExchangeScanId { get; set; }
		public int ItemId { get; set; }
		public ulong LastPrice { get; set; }
		public DateTime? LastSeen { get; set; }
	}
}
