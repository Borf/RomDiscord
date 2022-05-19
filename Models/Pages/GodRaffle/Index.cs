using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.GodRaffle
{
	public class Index
	{
		public List<GodEquip> GodEquip { get; set; } = new List<GodEquip>();
		public List<GodEquipGuildBinding> GodEquipGuild { get; set; } = new List<GodEquipGuildBinding>();
		public IReadOnlyCollection<SocketRole> Roles { get; set; } = null!;
		public SettingsModel Settings { get; internal set; } = null!;
		public IReadOnlyCollection<SocketGuildChannel> Channels { get; set; } = null!;
	}
}
