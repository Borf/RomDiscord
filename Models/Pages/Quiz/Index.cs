using Discord.WebSocket;

namespace RomDiscord.Models.Pages.Quiz
{
	public class Index
	{
		public SettingsModel Settings { get; set; } = null!;
		public IReadOnlyCollection<SocketGuildChannel> Channels { get; set; } = null!;
		public List<Db.Quiz> Quizes { get; internal set; } = null!;
	}
}
