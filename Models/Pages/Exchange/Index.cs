using Discord.WebSocket;
using RomDiscord.Models.Db;
using RomDiscord.Services;

namespace RomDiscord.Models.Pages.Exchange
{
	public class Index
	{
		public SettingsModel Settings { get; internal set; } = new SettingsModel();
		public List<SocketTextChannel> Channels { get; internal set; } = new List<SocketTextChannel>();
		public Guild Guild { get; internal set; } = null!;
		public Dictionary<ulong, List<ExchangePublicNotification>> PublicNotifications { get; internal set; } = null!;
		public ItemDb ItemDb { get; internal set; } = null!;
	}
}
