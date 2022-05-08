using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using System.Collections.Generic;
using System.Linq;

namespace RomDiscord.DiscordModules
{
	public class GodRaffleModule : InteractionModuleBase<SocketInteractionContext>
	{
		IServiceProvider services;

		public GodRaffleModule(IServiceProvider services)
		{
			this.services = services;
		}

		[SlashCommand("godequipraffle", "Rolls the raffle bot")]
		public async Task GodEquipRaffleAsync(
			[Summary(description: "Date of monday next week")] int startDay = -1, 
			[Summary(description: "Month of monday next week")] int startMonth = -1)
		{
			if (startDay == -1)
				startDay = DateTime.Now.Day;
			if (startMonth == -1)
				startMonth = DateTime.Now.Month;
			await RespondAsync("Raffling....", null, false, true);
			using var scope = services.CreateScope();
			var raffler = scope.ServiceProvider.GetRequiredService<GodEquipRaffle>();
			await raffler.RaffleWeek(new DateTime(DateTime.Now.Year, startMonth, startDay), this.Context.Guild);

			//await ReplyAsync("Done the weekly raffle");
		}

		[SlashCommand("godequipraffleperday", "Shows a post per day")]
		public async Task GodEquipPerDayAsync([Summary(description: "Date of monday next week")] bool emoji = true)
		{
			await DeferAsync(true);
			using var scope = services.CreateScope();
			var raffler = scope.ServiceProvider.GetRequiredService<GodEquipRaffle>();
			var embed = await raffler.BuildEmbed(DateTime.Now.Date, this.Context.Guild, emoji);
			await ModifyOriginalResponseAsync(m => m.Embed = embed);
		}




		public static List<RomDiscord.Services.TaskScheduler.ScheduledTask> GetTasks()
		{
			DateTime nextSunday = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek).Date;
			nextSunday = nextSunday.AddHours(17);

			return new List<RomDiscord.Services.TaskScheduler.ScheduledTask>()
			{
				new RomDiscord.Services.TaskScheduler.ScheduledTask()
				{
					NextRun = nextSunday, //next sunday
					TimeSpan = TimeSpan.FromDays(7), //every week
					Action = async (services) =>
					{
						using var scope = services.CreateScope();
						using var context = scope.ServiceProvider.GetRequiredService<Context>();
						var moduleSettings = scope.ServiceProvider.GetRequiredService<ModuleSettings>();
						var discord = services.GetRequiredService<DiscordSocketClient>();
						var godRaffle = scope.ServiceProvider.GetRequiredService<GodEquipRaffle>();
						var nextWeek = DateTime.Now.AddDays(2);
						var guilds = context.Guilds.ToList();
						foreach (var guild in guilds)
						{
							if (moduleSettings.GetBool(guild, "godraffle", "enabled", false))
							{
								var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
								await godRaffle.RaffleWeek(nextWeek, dcGuild);
								var channel = dcGuild.TextChannels.First(c => c.Id == moduleSettings.GetUlong(guild, "godraffle", "channel"));
								channel.SendMessageAsync(null, false, await godRaffle.BuildEmbed(nextWeek, dcGuild, moduleSettings.GetBool(guild, "godraffle", "emoji", true)));

							}
						}                   
					}
				}
			};
		}

	}
}