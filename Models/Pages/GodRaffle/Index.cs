using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.GodRaffle
{
	public class Index
	{
		public List<GodEquip> GodEquip { get; set; }
		public List<GodEquipGuildBinding> GodEquipGuild { get; set; }
		public IReadOnlyCollection<SocketRole> Roles { get; set; }
		public SettingsModel Settings { get; internal set; }
		public IReadOnlyCollection<SocketGuildChannel> Channels { get; set; }
	}
}
