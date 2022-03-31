namespace RomDiscord.Models.Db
{
	public class ModuleSetting
	{
		public int ModuleSettingId { get; set; }
		public Guild Guild { get; set; }
		public string Module { get; set; }
		public string Settings { get; set; }
		public string Value { get; set; }
	}
}
