using Discord.WebSocket;

namespace RomDiscord.Models.Pages.Quiz
{
	public class Index
	{
		public SettingsModel Settings { get; set; }
		public IReadOnlyCollection<SocketGuildChannel> Channels { get; set; }
		public List<Db.Quiz> Quizes { get; internal set; }
	}
}
