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
	public class GodRaffleModule : InteractionModuleBase<SocketInteractionContext>
	{
		IServiceProvider services;

		public GodRaffleModule(IServiceProvider services)
		{
			this.services = services;
		}

		[RequireUserPermission(GuildPermission.Administrator)]
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

		[RequireUserPermission(GuildPermission.Administrator)]
		[SlashCommand("godequipraffleperday", "Shows a post per day")]
		public async Task GodEquipPerDayAsync([Summary(description: "Date of monday next week")] bool emoji = true)
		{
			await DeferAsync(false);
			using var scope = services.CreateScope();
			var raffler = scope.ServiceProvider.GetRequiredService<GodEquipRaffle>();
			var embed = await raffler.BuildEmbed(DateTime.Now.Date, this.Context.Guild, emoji);
			await ModifyOriginalResponseAsync(m => m.Embed = embed);
		}





	}
}