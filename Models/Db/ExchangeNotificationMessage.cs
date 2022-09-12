using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class ExchangeNotificationMessage
	{
		[Key]
		public int ExchangeNotificationMessageId { get; set; }
		public ExchangePublicNotification ExchangePublicNotification { get; set; } = null!;
		public int ItemId { get; set; }
		public ulong Message { get; set; }
		public string Enchants { get; set; } = String.Empty;
		public int RefineLevel { get; set; } = 0;
		public bool Broken { get; set; }
	}
}
