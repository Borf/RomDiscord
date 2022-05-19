using Discord;
using Discord.WebSocket;
using RomDiscord.Models.Db;
using RomDiscord.Services;

namespace RomDiscord.Models.Pages.Emoji
{
	public class Index
	{
		public ModuleSettings Settings { get; set; } = null!;
		public IReadOnlyCollection<GuildEmote> Emoji { get; internal set; } = null!;
		public Guild Guild { get; set; } = null!;
	}
}
