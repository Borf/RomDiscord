using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Events
{
	public class EditModel
	{
		public Event Event { get; set; } = null!;
		public SocketTextChannel? Channel { get; set; } = null;
		public SocketGuild Guild { get; set; } = null!;
	}
}