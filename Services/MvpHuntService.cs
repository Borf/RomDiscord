using Discord.WebSocket;
using Humanizer;
using RomDiscord.Models.Db;

namespace RomDiscord.Services
{
	public class MvpHuntService
	{
		private readonly Context context;
		private readonly ModuleSettings moduleSettings;
		private readonly DiscordSocketClient discord;

		static Dictionary<int, string> mobNames = new Dictionary<int, string>
		{
			 { 30001, "Angeling"},
			 { 30002, "GTB"},
			 { 30003, "Deviling"},
			 { 30004, "Drake" },
			 { 30005, "Strouf" },
			 { 30006, "Goblin Leader" },
			 { 30007, "Mistress"},
			 { 30008, "Maya"},
			 { 30009, "Phreeoni"},
			 { 30011, "Eddga" },
			 { 30012, "Osiris" },
			 { 30013, "Moonlight"},
			 { 30014, "Orc Hero"},
			 { 30015, "Doppelganger"},
			 { 30016, "Orc Lord"},
			 { 30017, "Detarderous"},
			 { 30018, "Owl Baron" },
			 { 30019, "Chimera"},
			 { 30020, "Bloody Knight" },
			 { 30021, "Baphomet"},
			 { 30022, "Arc angeling"},
			 { 30023, "Atroce" },
			 { 30024, "Kobold Leader" },
			 { 30026, "hole filler"},
			 { 30028, "year"},
			 { 30029, "Dark Lord" },
			 { 30030, "Huge Deviling" },
			 { 30031, "Dracula"},
			 { 30032, "Randgris"},
			 { 30033, "Time Holder"},
			 { 30034, "Spashire"},
			 { 30035, "Kraken" },
			 { 30039, "Garm"},
			 { 30040, "Stormy Knight" },
			 { 30044, "Firelord Kaho" },
			 { 30045, "Arc Angeling"},
			 { 30048, "lord of death" },
			 { 30049, "Bloody Murderer"},
			 { 30050, "Wolf Grandma"},
			 { 30051, "Katerina" },
			 { 30052, "Sniper " },
			 { 30053, "Eremes" },
			 { 30054, "Ktullanux" },
			 { 30055, "Hill Wind" },
			 { 30056, "Gloom" },
			 { 30057, "Tahnee"},
			 { 30058, "Snake Gorgon" },
			 { 30059, "Potato"},
			 { 30060, "Wasteland Lord"},
			 { 30061,"Audhumbla" },
			 { 30062, "Yggseed" },
			 { 30063, "Soul Player" },
			 { 30064, "Devil Squid" },
			 { 30065, "Tao Gunka" },
			 { 30067, "Cait Sidhe" },
		};

		public MvpHuntService(Context context, ModuleSettings moduleSettings, DiscordSocketClient discord)
		{
			this.context = context;
			this.moduleSettings = moduleSettings;
			this.discord = discord;
		}

		public async Task UpdateChannel(string server, List<string> channels)
		{
			foreach (var channel in channels)
			{
				Console.WriteLine($"Updating MVP hunt for {channel}");
				var lastTime = context.MvpScans.Where(mvp => mvp.MvpId < 200000 && mvp.Channel == channel).Max(mvp => mvp.ScanTime);
				var mvps = context.MvpScans.Where(mvp => mvp.MvpId < 200000 && mvp.Channel == channel && mvp.ScanTime == lastTime).OrderBy(mvp => mvp.MvpId).ToList();
				if (mvps.Count == 0)
					continue;

				var msg = "MVPs for " + mvps.First().ScanTime + "```\n";
				for (int i = 0; i < mvps.Count; i += 3)
				{
					for (int col = 0; col < 3 && i + col < mvps.Count; col++)
						msg += ($"📛{(mobNames.ContainsKey(mvps[i + col].MvpId) ? mobNames[mvps[i + col].MvpId] : mvps[i + col].MvpId)}").PadRight(17);
					msg = msg.TrimEnd() + "\n";
					for (int col = 0; col < 3 && i + col < mvps.Count; col++)
						msg += ($"🔪{mvps[i + col].CharName}").PadRight(17);
					msg = msg.TrimEnd() + "\n";
					for (int col = 0; col < 3 && i + col < mvps.Count; col++)
						msg += ($"💓{TimeSpan.FromSeconds(mvps[i + col].AliveTime).Humanize()}").PadRight(17);
					msg = msg.TrimEnd() + "\n";
					msg += "\n";
					if (msg.Length > 1800)
						break;
				}
				msg += "```";

				string summary = "MVP Hunter Routes\n\n";
				foreach (var hunter in mvps.Select(mvp => mvp.CharName).Distinct())
				{
					var hunted = mvps.Where(mvp => mvp.CharName == hunter).OrderBy(mvp => mvp.AliveTime).Select(mvp => "`" + (mobNames.ContainsKey(mvp.MvpId) ? mobNames[mvp.MvpId] : (mvp.MvpId + "")) + "`").ToList();
					summary += "**" + hunter + "** -> " + String.Join(" -> ", hunted) + "\n";


				}


				foreach (var guild in context.Guilds.Where(g => g.DiscordGuildId != 0))
				{
					var scanMessage = moduleSettings.GetUlong(guild, "mvphunt", "msg_" + channel);
					var scanSummary = moduleSettings.GetUlong(guild, "mvphunt", "sum_" + channel);
					var scanTime = moduleSettings.GetUlong(guild, "mvphunt", "time_" + channel);
					var channelId = moduleSettings.GetUlong(guild, "mvphunt", channel);
					if (channelId == 0)
						continue;

					var dcChannel = discord.Guilds.First(g => g.Id == guild.DiscordGuildId).TextChannels.First(c => c.Id == channelId);

					if (scanTime != (ulong)mvps.First().ScanTime.Ticks || scanMessage == 0)
					{
						scanMessage = (await dcChannel.SendMessageAsync(msg)).Id;
						scanSummary = (await dcChannel.SendMessageAsync(summary)).Id;
						await moduleSettings.Set(guild, "mvphunt", "msg_" + channel, scanMessage + "");
						await moduleSettings.Set(guild, "mvphunt", "sum_" + channel, scanSummary + "");
						await moduleSettings.Set(guild, "mvphunt", "time_" + channel, mvps.First().ScanTime.Ticks + "");
					}
					else
					{
						await dcChannel.ModifyMessageAsync(scanMessage, m => m.Content = msg);
						if(scanSummary != 0)
							await dcChannel.ModifyMessageAsync(scanSummary, m => m.Content = summary);
						else
						{
							scanSummary = (await dcChannel.SendMessageAsync(summary)).Id;
							await moduleSettings.Set(guild, "mvphunt", "sum_" + channel, scanSummary + "");
						}
					}
				}
			}
		}
	}
}
