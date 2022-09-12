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
	public class HandbookController : Controller
	{
		private readonly ModuleSettings settings;
		private readonly DiscordSocketClient discord;
		private readonly Context context;

		public HandbookController(ModuleSettings settings, DiscordSocketClient discord, Context context)
		{
			this.settings = settings;
			this.discord = discord;
			this.context = context;
		}

		public async Task<IActionResult> Index()
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");

			var model = new Models.Pages.Handbook.Index()
			{
				Members = await context.Members.Where(m => m.Guild == guild).ToListAsync(),
				Channels = discord.Guilds.First(g => g.Id == guild.DiscordGuildId).TextChannels,
			};
			return View(model);
		}

	}
}
