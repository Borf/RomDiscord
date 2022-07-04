using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;

namespace RomDiscord.Services
{
	public class ExchangeService
	{
		private readonly DiscordSocketClient discord;
		private readonly Context context;
		private readonly ItemDb itemDb;

		public ExchangeService(DiscordSocketClient discord, Context context, ItemDb itemDb)
		{
			this.discord = discord;
			this.context = context;
			this.itemDb = itemDb;
		}

		internal async Task Update(ExchangeScan item)
		{
			var notifications = context.ExchangePublicNotifications.Include(epn => epn.Guild).Where(epn => epn.ItemId == item.ItemId);
			foreach(var notification in notifications)
			{
				var guild = discord.Guilds.First(g => g.Id == notification.Guild.DiscordGuildId);
				var channel = guild.TextChannels.First(c => c.Id == notification.ChannelId);

				EmbedBuilder builder = new EmbedBuilder()
					.WithColor(Color.Blue)
					.WithTitle(itemDb[item.ItemId].NameZh + " is now " + FormatPrice(item.Price))
					//                        .WithDescription($"{result.amount} availeble for {FormatPrice(result.price)}z")
					.WithThumbnailUrl(string.Format("https://borf.github.io/romicons/Items/{0}.png", item.ItemId))
					.AddField("Price", FormatPrice(item.Price), true)
					.AddField("Amount", item.Amount, true)
					;
//				if (item.snapTime.HasValue)
//					builder.AddField("Snaptime: ", result.snapTime.Value + ", in " + (result.snapTime.Value - DateTime.Now).Humanize());
//				if (!string.IsNullOrEmpty(result.message))
//					builder.AddField("Message", result.message);

				await channel.SendMessageAsync(null, false, builder.Build());
			}
		}

		static ulong message = 0;
		internal async Task Status(ExchangeScan lastScan)
		{
			var channel = discord.Guilds.First(g => g.Id == 819438757663997992).TextChannels.First(c => c.Id == 868966569814917152);
			if (message == 0)
				message = (await channel.SendMessageAsync("There are " + lastScan.Amount + " " + itemDb[lastScan.ItemId].NameZh + " on the exchange, for " + lastScan.Price + "z")).Id;
			else
				await channel.ModifyMessageAsync(message, m => m.Content = "There are " + lastScan.Amount + " " + itemDb[lastScan.ItemId].NameZh + " on the exchange, for " + lastScan.Price + "z");
		}
		static string FormatPrice(long price)
		{
			return price.ToString("N0").Replace(",", ".");
		}

	}
}
