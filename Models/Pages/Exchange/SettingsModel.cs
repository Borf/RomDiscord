using RomDiscord.Models.Db;
using RomDiscord.Services;

namespace RomDiscord.Models.Pages.Exchange
{
	public class SettingsModel
	{
		public SettingsModel(ModuleSettings moduleSettings, Guild guild)
		{
			Enabled = moduleSettings.GetBool(guild, "exchange", "enabled", false);
			LastChannel = moduleSettings.GetUlong(guild, "exchange", "lastChannel", 0);
		}
		public SettingsModel()
		{

		}

		public bool Enabled { get; set; } = false;
		public ulong LastChannel { get; set; } = 0;

	}
}
