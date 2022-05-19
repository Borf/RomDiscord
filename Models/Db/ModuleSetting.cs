using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class ModuleSetting
	{
		[Key]
		public int ModuleSettingId { get; set; }
		public Guild Guild { get; set; } = null!;
		public string Module { get; set; } = "";
		public string Settings { get; set; } = "";
		public string Value { get; set; } = "";
	}
}
