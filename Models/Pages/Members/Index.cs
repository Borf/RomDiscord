using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Members
{
	public class Index
	{
		public List<Member> Members { get; set; } = null!;
		public List<SocketGuildUser> DiscordMembers { get; set; } = null!;
	}
}
