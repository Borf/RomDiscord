using RomDiscord.Models.Db;
using RomDiscord.Services;

namespace RomDiscord.Models.Pages.HandBook
{
	public class SettingsModel
	{
		public SettingsModel(ModuleSettings moduleSettings, Guild guild)
		{
			Enabled = moduleSettings.GetBool(guild, "handbook", "enabled", false);
			Channel = moduleSettings.GetUlong(guild, "handbook", "channel", 0);
		}
		public SettingsModel()
		{

		}

		public bool Enabled { get; set; } = false;
		public ulong Channel { get; set; } = 0;

	}
}
