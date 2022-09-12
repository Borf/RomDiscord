using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Util;

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

		internal async Task Status(Models.Rest.Api.ExchangeScan lastScan)
		{

			lastScan.Entries = lastScan.Entries.Where(e => e.amount > 0).ToList();
			var notifications = context.ExchangePublicNotifications.Include(epn => epn.Guild).Where(epn => epn.ItemId == lastScan.itemId);
			foreach (var notification in notifications)
			{
				var guild = discord.Guilds.First(g => g.Id == notification.Guild.DiscordGuildId);
				var channel = guild.TextChannels.First(c => c.Id == notification.ChannelId);
				var messages = context.ExchangeNotifications.Where(n => n.ExchangePublicNotification == notification).ToList();
				foreach (var entry in lastScan.Entries)
				{
					var message = messages
						.FirstOrDefault(m =>
							m.Enchants == entry.enchant1 + "|" + entry.enchant2 + "|" + entry.enchant3 + "|" + entry.enchant4 &&
							m.RefineLevel == entry.refineLevel &&
							m.Broken == entry.broken &&
							m.ItemId == lastScan.itemId);
					if(message != null)
					{
						try
						{
							await channel.ModifyMessageAsync(message.Message, m => m.Embed = BuildEmbed(lastScan, entry));
						}catch(Exception)
						{
							context.ExchangeNotifications.Remove(message);
							await context.SaveChangesAsync();
							message = null;
						}

					}
					if (message == null)
					{
						var msg = await channel.SendMessageAsync(null, false, BuildEmbed(lastScan, entry));
						context.ExchangeNotifications.Add(new ExchangeNotificationMessage
						{
							ExchangePublicNotification = notification,
							Message = msg.Id,
							Enchants = entry.enchant1 + "|" + entry.enchant2 + "|" + entry.enchant3 + "|" + entry.enchant4,
							RefineLevel = entry.refineLevel,
							ItemId = lastScan.itemId
						});
						await context.SaveChangesAsync();
					}
				}

				foreach(var message in messages)
				{
					var enchants = message.Enchants.Split("|");
					if (!lastScan.Entries.Any(m => 
						lastScan.itemId == message.ItemId &&
						(m.enchant1 == enchants[0] || (string.IsNullOrEmpty(m.enchant1) && string.IsNullOrEmpty(enchants[0]))) && 
						(m.enchant2 == enchants[1] || (string.IsNullOrEmpty(m.enchant2) && string.IsNullOrEmpty(enchants[1]))) &&
						(m.enchant3 == enchants[2] || (string.IsNullOrEmpty(m.enchant3) && string.IsNullOrEmpty(enchants[2]))) &&
						(m.enchant4 == enchants[3] || (string.IsNullOrEmpty(m.enchant4) && string.IsNullOrEmpty(enchants[3]))) &&
						m.broken == message.Broken &&
						m.refineLevel == message.RefineLevel
						))
					{
						Console.WriteLine("Deleting message for item " + message.ItemId);
						try
						{
							await channel.DeleteMessageAsync(message.Message);
						}
						catch (Exception ex) { Console.WriteLine(ex); }
						context.ExchangeNotifications.Remove(message);
						await context.SaveChangesAsync();
					}
				}

			}

			foreach(var notification in context.ExchangePrivateNotifications.Where(n => n.ItemId == lastScan.itemId))
			{
				if (lastScan.Entries.Count > 0)
				{
					var user = await discord.GetUserAsync(notification.DiscordId);
					var channel = await user.CreateDMChannelAsync();
					await channel.SendMessageAsync(null, false, BuildEmbed(lastScan, lastScan.Entries[0]));
				}
				else
					Console.WriteLine("Not found!");
			}

		}

		private Embed BuildEmbed(Models.Rest.Api.ExchangeScan item, Models.Rest.Api.ExchangeScan.Entry entry)
		{
			EmbedBuilder builder = new EmbedBuilder()
								.WithColor(Color.Blue)
								.WithTitle(itemDb[item.itemId].NameZh + " is now " + FormatPrice(entry.price))
								//                        .WithDescription($"{result.amount} availeble for {FormatPrice(result.price)}z")
								.WithThumbnailUrl(string.Format("https://borf.github.io/romicons/Items/{0}.png", item.itemId))
								.WithFooter("Last scanned " + item.scanTime)
								;
			builder.AddField("Price", FormatPrice(entry.price));
			builder.AddField("Amount", entry.amount + "");
			if (entry.snapTime != null)
			{
				var unixtime = entry.snapTime.Value.ToUnixTimestamp();
				Console.WriteLine("TIme: " + unixtime);
				builder.AddField("Snaptime", $"<t:{unixtime}:T> <t:{unixtime}:R>");
			}
			return builder.Build();
		}

		static string FormatPrice(ulong price)
		{
			return price.ToString("N0").Replace(",", ".");
		}

	}
}
