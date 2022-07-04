using Discord.WebSocket;
using RomDiscord.Models.Db;
using RomDiscord.Services;

namespace RomDiscord.Models.Pages.MvpHunt
{
	public class Index
	{
		public ModuleSettings Settings { get; internal set; } = null!;
		public Guild Guild { get; internal set; } = null!;
		public List<SocketTextChannel> Channels { get; internal set; } = new List<SocketTextChannel>();
	}
}