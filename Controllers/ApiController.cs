using Discord.WebSocket;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RomDiscord.Models.Db;

namespace RomDiscord.Controllers
{
	[Route("api")]
	[ApiController]
	public class ApiController : ControllerBase
	{
		private readonly Context context;
		private readonly DiscordSocketClient discord;

		public ApiController(Context context, DiscordSocketClient discord)
		{
			this.context = context;
			this.discord = discord;
		}

		[HttpGet("test")]
		public async Task<IActionResult> Test()
		{
			return Ok("ok!");
		}


		static Dictionary<int, string> mobNames = new Dictionary<int, string>
		{
			 { 30001, "angeling"},
			 { 30002, "golden thief bug"},
			 { 30003, "deviling"},
			 { 30004, "drake" },
			 { 30005, "strouf" },
			 { 30006, "goblin leader" },
			 { 30007, "mistress"},
			 { 30008, "maya"},
			 { 30009, "phreeoni"},
			 { 30011, "eddga" },
			 { 30012, "osiris" },
			 { 30013, "moonlight flower"},
			 { 30014, "orc hero"},
			 { 30015, "doppelganger"},
			 { 30016, "orc lord"},
			 { 30017, "detarderous"},
			 { 30018, "owl baron" },
			 { 30019, "chimera"},
			 { 30020, "bloody knight" },
			 { 30021, "baphomet"},
			 { 30022, "arc angeling"},
			 { 30023, "atroce" },
			 { 30024, "kobold leader" },
			 { 30026, "hole filler"},
			 { 30028, "year"},
			 { 30029, "dark lord" },
			 { 30030, "huge deviling" },
			 { 30031, "dracula"},
			 { 30032, "randgris"},
			 { 30033, "time holder"},
			 { 30034, "spashire"},
			 { 30035, "kraken" },
			 { 30039, "garm"},
			 { 30040, "stormy knight" },
			 { 30041, "santa pirate - cat ufo mkii"},
			 { 30042, "primeval boss" },
			 { 30043, "year"},
			 { 30044, "firelord kaho" },
			 { 30045, "arc angeling"},
			 { 30048, "lord of death" },
			 { 30049, "bloody murderer"},
			 { 30050, "wolf grandma"},
			 { 30051, "katerina" },
			 { 30052, "sniper " },
			 { 30053, "eremes" },
			 { 30054, "ktullanux" },
			 { 30055, "hill wind" },
			 { 30056, "gloom" },
			 { 30057, "tahnee"},
			 { 30058, "snake gorgon" },
			 { 30059, "potato"},
			 { 30060, "wasteland lord"},
			 { 30061,"audhumbla" },

		};

		static ulong scanMessage = 0;
		[HttpPost("MvpScan")]
		public async Task<IActionResult> MvpScan([FromBody] List<MvpScan> data)
		{
			foreach (var scan in data)
			{
				var s = context.MvpScans.FirstOrDefault(s => s.ScanTime == scan.ScanTime && s.MvpId == scan.MvpId && s.Channel == scan.Channel);
				if (s == null)
					context.MvpScans.Add(scan);
				else
				{
					s.CharName = scan.CharName;
					s.AliveTime = scan.AliveTime;
				}
			}
			await context.SaveChangesAsync();

			var en1 = data;//.Where(d => d.Channel == "1").ToList();
			if (en1.Count > 0)
			{
				var channel = discord.Guilds.First(g => g.Id == 819438757663997992).TextChannels.First(c => c.Id == 868966569814917152);
				var msg = "```ruby\n";
				for (int i = 0; i < en1.Count; i += 3)
				{
					for (int col = 0; col < 3 && i + col < en1.Count; col++)
						msg += ($"📛{(mobNames.ContainsKey(en1[i + col].MvpId) ? mobNames[en1[i + col].MvpId] : en1[i + col].MvpId)}").PadRight(17);
					msg = msg.TrimEnd() + "\n";
					for (int col = 0; col < 3 && i + col < en1.Count; col++)
						msg += ($"🔪{en1[i + col].CharName}").PadRight(17);
					msg = msg.TrimEnd() + "\n";
					for (int col = 0; col < 3 && i + col < en1.Count; col++)
						msg += ($"💓{TimeSpan.FromSeconds(en1[i+col].AliveTime).Humanize()}").PadRight(17);
					msg = msg.TrimEnd() + "\n";
					msg += "\n";
					if (msg.Length > 1800)
						break;
				}
				msg += "```";

				if (scanMessage == 0)
					scanMessage = (await channel.SendMessageAsync(msg)).Id;
				else
					await channel.ModifyMessageAsync(scanMessage, m => m.Content = msg);


			}

			return Ok("Ok");
		}
	}
}
