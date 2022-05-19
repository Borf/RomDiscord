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
	public class EmojiController : Controller
	{
		private readonly ModuleSettings settings;
		private readonly DiscordSocketClient discord;
		private readonly Context context;

		public EmojiController(ModuleSettings settings, DiscordSocketClient discord, Context context)
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
			return View(new RomDiscord.Models.Pages.Emoji.Index()
			{
				Emoji = discord.Guilds.First(g => g.Id == guild?.DiscordGuildId).Emotes,
				Settings = settings,
				Guild = guild
			});
		}

		[HttpPost("UpdateJobEmoji/{job}/{emoji}")]
		public async Task<IActionResult> UpdateJobEmoji(Job job, ulong emoji)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			await settings.Set(guild, "emoji", job.ToString(), emoji + "");
			return Ok("Ok");
		}

	}
}
