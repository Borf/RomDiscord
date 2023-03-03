using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class ExchangeNotificationMessage
	{
		[Key]
		public int ExchangeNotificationMessageId { get; set; }
		public ExchangePublicNotification ExchangePublicNotification { get; set; } = null!;
		public ulong DiscordMessageId { get; set; }
		public int ItemId { get; set; }
		public string Guid { get; set; }
	}
}
