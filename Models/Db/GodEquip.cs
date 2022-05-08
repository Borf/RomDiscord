using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class GodEquip
	{
		[Key]
		public int GodEquipId { get; set; }
		public string Name { get; set; } = "";
		public string Type { get; set; } = "";
	}
}
