using Discord.WebSocket;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.HandBook;

namespace RomDiscord.Models.Pages.Handbook
{
	public class Index
	{
		public List<Member> Members { get; set; } = null!;
		public IReadOnlyCollection<SocketTextChannel> Channels { get; set; } = null!;
		public SettingsModel Settings { get; set; } = new SettingsModel();
	}
}
