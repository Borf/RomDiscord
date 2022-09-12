using Microsoft.AspNetCore.Mvc;
using RomDiscord.Models.Db;

namespace RomDiscord.Util
{
	public static class Util
	{
		public static string NameToId(string name)
		{
			name = name.ToLower();
			name = name.Trim();
			name = name.Replace(" ", "");
			name = name.Replace("'", "");
			return name;
		}

		public static Guild? Guild(this Controller controller, Context context)
		{
			if (controller.HttpContext.SessionData().ActiveGuild == null)
				return null;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
			return context.Guilds.FirstOrDefault(g => g.DiscordGuildId == controller.HttpContext.SessionData().ActiveGuild.Id);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
		}
		public static long ToUnixTimestamp(this DateTime value)
		{
			return (long)(value.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds;
		}


		public static string Color(this Job cls)
		{
			switch(cls)
			{
				case Job.ArcaneMaster:
				case Job.Runemaster:
				case Job.BladeSoul:
				case Job.PhantomDance:
				case Job.StellarHunter:
				case Job.SolarTrouvere:
				case Job.LunaDanseuse:
				case Job.DragonFist:
				case Job.Lightbringer:
				case Job.SoulBinder:
				case Job.NoviceGuardian:
				case Job.InfernoArmor:
				case Job.Yamata:
				case Job.Ryuoumaru:
				case Job.Slayer:
				case Job.SpiritWhisperer:
				case Job.Tyrant:
					return "#FF0000";

				case Job.DivineAvenger:
				case Job.Saint:
				case Job.Begetter:
				case Job.Chronomancer:
					return "#00FF00";
			}
			return "";
		}

		public static string Name(this Job cls)
		{
			switch (cls)
			{
				case Job.ArcaneMaster:		return "Arcane Master";
				case Job.Runemaster:		return "Rune Master";
				case Job.BladeSoul:			return "Blade Soul";
				case Job.PhantomDance:		return "Phantom Dance";
				case Job.StellarHunter:		return "Stellar Hunter";
				case Job.SolarTrouvere:		return "Solar Trouvere";
				case Job.LunaDanseuse:		return "Luna Danseuse";
				case Job.DragonFist:		return "Dragon Fist";
				case Job.Lightbringer:		return "Lightbringer";
				case Job.SoulBinder:		return "Soul Binder";
				case Job.NoviceGuardian:	return "Novice Guardian";
				case Job.InfernoArmor:		return "Inferno Armor";
				case Job.Yamata:			return "Yamata";
				case Job.Ryuoumaru:			return "Ryuoumaru";
				case Job.Slayer:			return "Slayer";
				case Job.SpiritWhisperer:	return "Cat";
				case Job.Tyrant:			return "Tyrant";
				case Job.DivineAvenger:		return "Divine Avenger";
				case Job.Saint:				return "Saint";
				case Job.Begetter:			return "Begetter";
				case Job.Chronomancer:		return "Chronomancer";
			}
			return "";
		}
	}
}
