using RomDiscord.Models.Db;
using RomDiscord.Services;

namespace RomDiscord.Models.Pages.Events
{
	public class SettingsModel
	{
		public SettingsModel(ModuleSettings moduleSettings, Guild guild)
		{
			Enabled = moduleSettings.GetBool(guild, "events", "enabled", false);
			Channel = moduleSettings.GetUlong(guild, "events", "channel", 0);
		}
		public SettingsModel()
		{

		}

		public bool Enabled { get; set; } = false;
		public ulong Channel { get; set; } = 0;

	}
}
