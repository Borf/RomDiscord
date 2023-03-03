using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomDiscord.Models.Db
{
	public class ExchangePublicNotification
	{
		[Key]
		public int ExchangePublicNotificationId { get; set; }
		public Guild Guild { get; set; } = null!;
		public ulong ChannelId { get; set; }
		public int ItemId { get; set; }
		public Enchant Enchant { get; set; } = Enchant.None;
		public int MinEnchantLevel { get; set; } = 0;
        public int MinRefineLevel { get; set; } = 0;
        public List<ExchangeNotificationMessage> Messages { get; set; } = new List<ExchangeNotificationMessage>(); 
	}
}
