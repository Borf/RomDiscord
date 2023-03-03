using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class ExchangePrivateNotification
	{
		[Key]
		public int ExchangePrivateNotificationId { get; set; }
		public ulong DiscordId { get; set; }
		public int ItemId { get; set; }
        public Enchant Enchant { get; set; } = Enchant.None;
        public int MinEnchantLevel { get; set; } = 0;
		public int MinRefineLevel { get; set; } = 0;
    }
}
