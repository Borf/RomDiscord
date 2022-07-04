using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using RomDiscord.Util;

namespace RomDiscord.Controllers
{

	[Route("[controller]")]
	[Controller]
	public class MvpHuntController : Controller
	{
		private readonly ModuleSettings settings;
		private readonly DiscordSocketClient discord;
		private readonly Context context;

		public MvpHuntController(ModuleSettings settings, DiscordSocketClient discord, Context context)
		{
			this.settings = settings;
			this.discord = discord;
			this.context = context;
		}

		public IActionResult Index()
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
			return View(new Models.Pages.MvpHunt.Index()
			{
				Settings = settings,
				Guild = guild,
				Channels = dcGuild.TextChannels.OrderBy(c => c.Position).ToList()
			});
		}

		[HttpPost("UpdateChannel/{channel}/{dcChannel}")]
		public async Task<IActionResult> UpdateChannel(string channel, ulong dcChannel)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			await settings.Set(guild, "mvphunt", channel, dcChannel + "");
			return Ok("Ok");
		}

	}
}
