using RomDiscord.Models.Db;

namespace RomDiscord.Services
{
	public class ModuleSettings
	{
		private readonly Context context;

		public ModuleSettings(Context context)
		{
			this.context = context;
		}

		public async Task Set(Guild guild, string module, string setting, string value)
		{
			var s = context.ModuleSettings.FirstOrDefault(s => s.Guild == guild && s.Module == module && s.Settings == setting);
			if (s == null)
			{
				s = new ModuleSetting()
				{
					Guild = guild,
					Module = module,
					Settings = setting,
					Value = value
				};
				context.ModuleSettings.Add(s);
			}
			else
				s.Value = value;
			await context.SaveChangesAsync();
		}

		public string Get(Guild guild, string module, string setting, string defaultValue = "")
		{
			var s = context.ModuleSettings.FirstOrDefault(s => s.Guild == guild && s.Module == module && s.Settings == setting);
			if (s == null)
				return defaultValue;
			else
				return s.Value;
		}
		public double GetDouble(Guild guild, string module, string setting, double defaultValue = 0)
		{
			return double.Parse(Get(guild, module, setting, defaultValue + ""));
		}
		public float GetFloat(Guild guild, string module, string setting, float defaultValue = 0)
		{
			return float.Parse(Get(guild, module, setting, defaultValue + ""));
		}
		public int GetInt(Guild guild, string module, string setting, int defaultValue = 0)
		{
			return int.Parse(Get(guild, module, setting, defaultValue + ""));
		}
		public ulong GetUlong(Guild guild, string module, string setting, ulong defaultValue = 0)
		{
			return ulong.Parse(Get(guild, module, setting, defaultValue + ""));
		}
		public bool GetBool(Guild guild, string module, string setting, bool defaultValue)
		{
			return bool.Parse(Get(guild, module, setting, defaultValue+""));
		}
	}
}
