using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Members
{
	public class EditModel
	{
		public Member Member { get; set; } = null!;
		public List<SocketGuildUser> DiscordMembers { get; set; } = new List<SocketGuildUser>();
	}
}
