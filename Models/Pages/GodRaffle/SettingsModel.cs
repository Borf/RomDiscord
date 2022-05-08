using RomDiscord.Models.Db;
using RomDiscord.Services;

namespace RomDiscord.Models.Pages.GodRaffle
{
	public class SettingsModel
	{
		public SettingsModel(ModuleSettings moduleSettings, Guild guild)
		{
			Enabled = moduleSettings.GetBool(guild, "godraffle", "enabled", false);
			Emoji = moduleSettings.GetBool(guild, "godraffle", "emoji", true);
			TimeFactor = moduleSettings.GetFloat(guild, "godraffle", "timefactor", 7);
			MaxTimeFactor = moduleSettings.GetFloat(guild, "godraffle", "maxtimefactor", 2);
			GuildMemberRole = moduleSettings.GetUlong(guild, "godraffle", "guildmemberrole", 0);
			DonateRole = moduleSettings.GetUlong(guild, "godraffle", "donaterole", 0);
			Channel = moduleSettings.GetUlong(guild, "godraffle", "channel", 0);
			DaysEnabled = moduleSettings.Get(guild, "godraffle", "daysenabled","").Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();
		}
		public SettingsModel()
		{

		}

		public bool Enabled { get; set; } = false;
		public bool Emoji { get; set; } = true;
		public float TimeFactor { get; set; } = 7;
		public float MaxTimeFactor { get; set; } = 2;
		public ulong GuildMemberRole { get; set; } = 0;
		public ulong DonateRole { get; set; } = 0;
		public ulong Channel { get; set; } = 0;

		public List<int> DaysEnabled { get; set; } = new List<int>();

	}
}
