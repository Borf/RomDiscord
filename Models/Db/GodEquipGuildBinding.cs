using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class GodEquipGuildBinding
	{
		[Key]
		public int GodEquipGuildBindingId { get; set; }
		public Guild Guild { get; set; }
		public GodEquip GodEquip { get; set; }
		public int Level { get; set; } = 1;
		public int Amount { get; set; }
		public ulong DiscordRoleId { get; set; }
		public string Emoji { get; set; }
	}
}
