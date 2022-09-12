using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using System.Collections.Generic;
using System.Linq;

namespace RomDiscord.DiscordModules
{
	public class OccupationModule : InteractionModuleBase<SocketInteractionContext>
	{
		IServiceProvider services;

		public OccupationModule(IServiceProvider services)
		{
			this.services = services;
		}

		[SlashCommand("agitstatus", "Shows the current status of agits")]
		public async Task AgitStatusAsync()
		{
			await DeferAsync(true);
			using var scope = services.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<Context>();

			var eb = new EmbedBuilder();
			var lastTime = context.OccupationScans.Max(o => o.ScanTime);

			Dictionary<int, string> castleNames = new Dictionary<int, string>()
			{
				{1005, "Valk Bg" },
				{1006, "Valk Md" },
				{1007, "Valk Sm" },
				{2005, "Brit Bg" },
				{2006, "Brit Md" },
				{2007, "Brit Sm" },
				{3005, "GrWd Bg" },
				{3006, "GrWd Md" },
				{3007, "GrWd Sm" },
				{4005, "Luin Bg" },
				{4006, "Luin Md" },
				{4007, "Luin Sm" },
			};

			Dictionary<string, string> fronts = new Dictionary<string, string>()
			{
				{ "EN1","Front 1" },
				{ "EN2","Front 1" },
				{ "RU1","Front 4" },
				{ "RU2","Front 4" },
				{ "RU3","Front 5" },
				{ "RU4","Front 3" },
				{ "TR1","Front 2" },
				{ "PT1","Front 5" },
				{ "ES1","Front 1" },
				{ "ES2","Front 2" },
				{ "DE1","Front 2" },
				{ "DE2","Front 4" },
				{ "FR1","Front 3" },
				{ "FR2","Front 3" },
			};

			var scans = context.OccupationScans.Where(o => o.ScanTime == lastTime).ToList();
			var byChannel = scans.GroupBy(kv => kv.Channel, kv => kv);
			var done = new List<string>();

			foreach (var kv in byChannel.OrderBy(kv => fronts[kv.Key]))
			{
				if (done.Contains(fronts[kv.Key]))
					continue;
				done.Add(fronts[kv.Key]);
				string line = "";
				foreach (var castle in kv.OrderBy(c => c.CastleId))
					line += "`" + castleNames[castle.CastleId].PadRight(10) + "`" + castle.GuildName + "\n";
				eb.AddField(fronts[kv.Key], line, true);
			}
			int count = 3 - (eb.Fields.Count % 3);
			for (int i = 0; i < count; i++)
				eb.AddField("\u200b", "\u200b", true);


			await ModifyOriginalResponseAsync(m => m.Embed = eb.Build());
		}




	}
}