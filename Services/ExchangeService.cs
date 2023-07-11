using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Rest.Api;
using RomDiscord.Util;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Reflection.PortableExecutable;
using System.Reflection;

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

	/*	internal async Task Status(Models.Rest.Api.ExchangeScan lastScan)
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

		}*/

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

        internal async Task NewScanResult(NewExchangeScan data)
        {
            string itemName = data.ItemId + "";
            if (itemDb.db.ContainsKey(data.ItemId))
                itemName = itemDb[data.ItemId].NameZh;
			//Console.WriteLine("Got an update for " + itemName);
            var notifications = context.ExchangePublicNotifications.Include(epn => epn.Guild).Include(epm => epm.Messages).Where(epn => epn.ItemId == data.ItemId || epn.ItemId == 0).ToList();
            foreach (var notification in notifications)
			{
                var guild = discord.Guilds.First(g => g.Id == notification.Guild.DiscordGuildId);
                var channel = guild.TextChannels.First(c => c.Id == notification.ChannelId);

                var messages = notification.Messages.Where(n => n.ItemId == data.ItemId).ToList();
                foreach (var message in messages)
				{
					if (data.Data.Any(d => d.Guid == message.Guid))
						continue;
                    context.ExchangeNotifications.Remove(message);
                    var dcMsg = await channel.GetMessageAsync(message.DiscordMessageId);
					try
					{
						await channel.DeleteMessageAsync(dcMsg);
					}
					catch (Exception) { }
                }
				await context.SaveChangesAsync();

				if(data.Data.Count == 0)
				{
                    foreach (var message in messages)
                    {
                        var dcMsg = await channel.GetMessageAsync(message.DiscordMessageId);
						try
						{
							await channel?.DeleteMessageAsync(dcMsg);
						}
						catch (Exception) { }
                        context.ExchangeNotifications.Remove(message);
                    }
                    await context.SaveChangesAsync();
                }

                foreach (var item in data.Data)
				{
					if (messages.Count(n => n.Guid == item.Guid) > 1)
					{
						context.ExchangeNotifications.RemoveRange(messages.Where(n => n.Guid == item.Guid));
						await context.SaveChangesAsync();
						foreach (var m in messages.Where(n => n.Guid == item.Guid))
						{
							try
							{
								await channel.DeleteMessageAsync(m.DiscordMessageId);
							}
							catch (Exception) { }
						}
						messages = messages.Where(n => n.Guid != item.Guid).ToList();
                    }
                    var message = messages.FirstOrDefault(n => n.Guid == item.Guid);

                    if(message != null)
					{ 
						var dcMsg = await channel.GetMessageAsync(message.DiscordMessageId);
						if (dcMsg != null) //if there's already a discord message for this filter, with the same guid, then I don't think we need to recheck the filters. Gotta make sure the guids are unique though and persistent
						{
							//var task = channel.ModifyMessageAsync(message.DiscordMessageId, c => c.Embed = BuildEmbed2(data, item));
                            Console.WriteLine("Done Updating..." + itemName);
                            continue;
						}
						else //there should have been a message but it's not on discord. Did someone manually delete it?
						{
							Console.WriteLine("No message found for " + itemName);
							context.ExchangeNotifications.Remove(message);
							await context.SaveChangesAsync();
						}
                    }
					//filtering
                    if (notification.ItemId == 0)
					{
						if ((notification.MinRefineLevel > 0 && item.RefineLevel == null) || (item.RefineLevel != null && notification.MinRefineLevel > (item.RefineLevel ?? 0)))
							continue;
						if (notification.Enchant != Enchant.None || notification.MinEnchantLevel > 0)
						{
							if (item.Enchants == null)
								continue;
							if (item.Enchants.Count != 4)
								continue;
							if ((int)item.Enchants[3].Enchant / 10 != (int)notification.Enchant / 10)
								continue;

							if (notification.MinEnchantLevel > (int)item.Enchants[3].Enchant % 10)
								continue;
						}
						if (notification.MinRefineLevel == 0 && notification.Enchant == Enchant.None && notification.MinEnchantLevel == 0)
							continue;
					}

					var msg = await channel.SendMessageAsync(null, false, BuildEmbed2(data, item));
					context.ExchangeNotifications.Add(new ExchangeNotificationMessage()
					{
						DiscordMessageId = msg.Id,
						ExchangePublicNotification = notification,
						Guid = item.Guid,
						ItemId = data.ItemId
					});
                    await context.SaveChangesAsync();
                }
            }


            foreach (var notification in context.ExchangePrivateNotifications.Where(n => n.ItemId == data.ItemId))
            {
                if (data.Data.Count > 0)
                {
                    var user = await discord.GetUserAsync(notification.DiscordId);
                    var channel = await user.CreateDMChannelAsync();
                    await channel.SendMessageAsync(null, false, BuildEmbed2(data, data.Data[0]));
                }
                else
                    Console.WriteLine("Not found!");
            }


        }

        public async Task NewScanItemList(List<int> itemsScanned)
        {
			var messages = context.ExchangeNotifications.Include(en => en.ExchangePublicNotification).ThenInclude(n => n.Guild).Where(en => !itemsScanned.Contains(en.ItemId));
			foreach(var message in messages)
			{
				var discordMessage = await discord.GetGuild(message.ExchangePublicNotification.Guild.DiscordGuildId).GetTextChannel(message.ExchangePublicNotification.ChannelId).GetMessageAsync(message.DiscordMessageId);
				if (discordMessage != null)
				{
					await discordMessage.DeleteAsync();
					Console.WriteLine("Removing message for " + itemDb[message.ItemId].NameZh);
				}
			    context.ExchangeNotifications.Remove(message);
            }
            await context.SaveChangesAsync();
        }


        private Embed BuildEmbed2(Models.Rest.Api.NewExchangeScan item, Models.Rest.Api.NewExchangeScan.ItemData itemData)
        {
			string itemName = item.ItemId + "";
			if (itemDb.db.ContainsKey(item.ItemId))
				itemName = itemDb[item.ItemId].NameZh;
			if (itemData.RefineLevel > 0)
				itemName = "+" + itemData.RefineLevel + " " + itemName;
            EmbedBuilder builder = new EmbedBuilder()
                                .WithColor(Color.Blue)
                                .WithTitle(itemName + " is now " + FormatPrice(itemData.Price))
                                //                        .WithDescription($"{result.amount} availeble for {FormatPrice(result.price)}z")
                                .WithThumbnailUrl(string.Format("https://borf.github.io/romicons/Items/{0}.png", item.ItemId))
                                .WithFooter("Last scanned " + item.ScanTime + " by borf")
                                ;
            builder.AddField("Price", FormatPrice(itemData.Price), true);
            builder.AddField("Amount", FormatPrice(itemData.Amount) + "", true);
            if (itemData.EndTime != 0)
            {
                builder.AddField("Snaptime", $"<t:{itemData.EndTime}:T> <t:{itemData.EndTime}:R>");
            }
            if (itemData.RefineLevel > 0)
                builder.AddField("Refine level", "+" + itemData.RefineLevel, true);
//            builder.AddField("Item ID", item.ItemId, true);
//			if(!string.IsNullOrEmpty(itemData.Guid))
//				builder.AddField("ID", itemData.Guid, true);
            if (itemData.Enchants != null && itemData.Enchants.Count > 0)
			{
				string enchants = "";
				foreach (var enchant in itemData.Enchants)
					enchants += BuildEnchant(enchant) + "\n";
				builder.AddField("Enchants", enchants);
			}

            return builder.Build();
        }

        private string BuildEnchant(NewExchangeScan.EnchantInfo enchant)
        {
			Enchant baseEnchant = enchant.Enchant;
			int level = 0;
            if (enchant.Enchant >= Enchant.Focus && enchant.Enchant <= Enchant.AntiMage4_)
            {
                level = (int)enchant.Enchant % 10;
                baseEnchant = (Enchant)(((int)enchant.Enchant / 10) * 10);

            }
            switch (baseEnchant)
			{
				case Enchant.Str:					return "Str +" +				(enchant.Level / 1.0);
				case Enchant.Vit:					return "Vit +" +				(enchant.Level / 1.0);
				case Enchant.Dex:					return "Dex +" +				(enchant.Level / 1.0);
				case Enchant.Agi:					return "Agi +" +				(enchant.Level / 1.0);
				case Enchant.Int:					return "Int +" +				(enchant.Level / 1.0);
				case Enchant.Luk:					return "Luk +" +				(enchant.Level / 1.0);
				case Enchant.MaxHp:					return "MaxHP +" +				(enchant.Level / 1.0);
				case Enchant.MaxHpPer:				return "MaxHP% +" +				(enchant.Level / 10.0) + "%";
				case Enchant.MaxSp:					return "MaxSP +" +				(enchant.Level / 1.0);
				case Enchant.MaxSpPer:				return "MaxSP% +" +				(enchant.Level / 10.0) + "%";
				case Enchant.Atk:					return "Atk +" +				(enchant.Level / 1.0);
				case Enchant.MAtk:					return "MAtk +" +				(enchant.Level / 1.0);
				case Enchant.Def:					return "Def +" +				(enchant.Level / 1.0);
				case Enchant.MDef:					return "MDef +" +				(enchant.Level / 1.0);
				case Enchant.Hit:					return "Hit +" +				(enchant.Level / 1.0);
				case Enchant.Critical:				return "Critical +" +			(enchant.Level / 1.0);
				case Enchant.Flee:					return "Flee +" +				(enchant.Level / 1.0);
				case Enchant.CritDef:				return "Crit.Def +" +			(enchant.Level / 10.0) + "%";
				case Enchant.CritDmg:				return "Crit.Dmg +" +			(enchant.Level / 10.0) + "%";
				case Enchant.CritRes:				return "Crit.Res +" +			(enchant.Level / 1.0);
				case Enchant.HealingReceived:		return "Healing Received +" +	(enchant.Level / 10.0) + "%";
				case Enchant.HealingIncrease:		return "Healing Increase +" +	(enchant.Level / 10.0) + "%";
				case Enchant.PhyDmgInc:				return "Phy. Dmg Inc +" +		(enchant.Level / 10.0) + "%";
				case Enchant.AttackSpeed:			return "Attack Spd +" +			(enchant.Level / 10.0) + "%";
				case Enchant.SilenceRes:			return "Silence Res +" +		(enchant.Level / 10.0) + "%";
				case Enchant.FreezeRes:				return "Freeze Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.StoneRes:				return "Stone Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.StunRes:				return "Stun Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.BlindRes:				return "Blind Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.PoisonRes:				return "Poison Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.SnareRes:				return "Snare Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.FearRes:				return "Fear Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.CurseRes:				return "Curse Res +" +			(enchant.Level / 10.0) + "%";
				case Enchant.DmgReduc:				return "Dmg Reduc +" +			(enchant.Level / 10.0) + "%";


                case Enchant.Focus:					return "Focus " +			level + " (Chant disrupts durability " +	(2.0 * level) + "%)";
				case Enchant.Morale:				return "Morale " +			level + " (Ignore Def +" +					(5.0 * level) + "%)";
				case Enchant.Magic:					return "Magic " +			level + " (Shorten CT Variable +" +			(2.5 * level) + "%)";
				case Enchant.Arch:					return "Arch " +			level + " (Ranged Atk Inc. +" +				(2.5 * level) + "%)";
				case Enchant.Sharp:					return "Sharp " +			level + " (Crit.Dmg +" +					(5.0 * level) + "%)";
				case Enchant.Tenacity:				return "Tenacity " +		level + " (Physical Dmg Reduc +" +			(2.5 * level) + "%)";
				case Enchant.Patience:				return "Patience " +		level + " (CC Reduction " +					(5.0 * level) + "%)";
				case Enchant.AntiMage:				return "Anti-mage " +		level + " (Ignore M.Def " +					(5.0 * level) + "%)";
				case Enchant.SharpBlade:			return "Sharp Blade " +		level + " (Melee Atk Inc +" +				(2.5 * level) + "%)";
				case Enchant.Arcane:				return "Arcane " +			level + " (M.Dmg +" +						(2.0 * level) + "%)";
				case Enchant.DivineBlessing:		return "Divine Blessing " + level + " (Magic Reduc +" +					(2.5 * level) + "%)";
				case Enchant.Armor:					return "Armor " +			level + " (Crit Res +" +					(5.0 * level) + ")";
				case Enchant.Zeal:					return "Zeal " +			level + " (Auto Attack Dmg Inc +" +			(2.5 * level) + "%)";
				case Enchant.Insight:				return "Insight " +			level + " (Ignore MDef +" +					(5.0 * level) + "%)";
				case Enchant.Blasphemy:				return "Blasphemy " +		level + " (Skill Dmg Reduc +" +				(2.5 * level) + "%)";
				case Enchant.ArmorBreaking:			return "Armor Breaking " +	level + " (Pen +" +							(1.5 * level) + "%)";
				case Enchant.AntiMage_:				return "Anti-mage " +		level + " (MPen +" +						(1.5 * level) + "%)"; //tuna talisman (acce)
					//blasphemy	skill dmg reduc
				default:							return enchant.Enchant + " " + enchant.Level;
            }



        }
    }
}
