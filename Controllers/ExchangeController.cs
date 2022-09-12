using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.Exchange;
using RomDiscord.Services;
using RomDiscord.Util;

namespace RomDiscord.Controllers
{

	[Route("[controller]")]
	[Controller]
	public class ExchangeController : Controller
	{
		private readonly ModuleSettings settings;
		private readonly DiscordSocketClient discord;
		private readonly Context context;
		private readonly ItemDb itemDb;

		public ExchangeController(ModuleSettings settings, DiscordSocketClient discord, Context context, ItemDb itemDb)
		{
			this.settings = settings;
			this.discord = discord;
			this.context = context;
			this.itemDb = itemDb;
		}

		public IActionResult Index()
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
			return View(new Models.Pages.Exchange.Index()
			{
				Settings = new SettingsModel(settings, guild),
				Guild = guild,
				Channels = dcGuild.TextChannels.OrderBy(c => c.Position).ToList(),
				PublicNotifications = context.ExchangePublicNotifications.Where(n => n.Guild == guild).ToList().OrderBy(n => n.ChannelId),
				ItemDb = itemDb
			});
		}

		[HttpPost("AddItem")]
		public async Task<IActionResult> AddItem(int ItemId, ulong ChannelId)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			if (ItemId != 0)
			{
				await settings.Set(guild, "exchange", "lastChannel", ChannelId + "");

				context.ExchangePublicNotifications.Add(new ExchangePublicNotification()
				{
					ItemId = ItemId,
					ChannelId = ChannelId,
					Guild = guild
				});
				await context.SaveChangesAsync();
			}
			return RedirectToAction("Index");
		}


		[HttpPost("ChangeChannel/{notificationId}/{channelId}")]
		public async Task<IActionResult> ChangeChannel(int notificationId, ulong channelId)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var notification = await context.ExchangePublicNotifications.FindAsync(notificationId);
			if(notification != null)
				notification.ChannelId = channelId;
			await context.SaveChangesAsync();
			return Ok("ok");
		}

	}
}
