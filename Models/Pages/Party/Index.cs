using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Party
{
	public class Index
	{
		public List<Member> Members { get; set; } = null!;
		public List<RomDiscord.Models.Db.Party> Parties { get; set; } = null!;
		public IReadOnlyCollection<SocketGuildChannel> Channels { get; set; } = null!;
		public ulong ActiveChannel { get; internal set; }
	}
}
