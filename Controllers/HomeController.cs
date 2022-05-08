using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RomDiscord.Models;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages;
using RomDiscord.Util;
using System.Diagnostics;

namespace RomDiscord.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly Context context;
		private readonly DiscordSocketClient discord;
		private readonly IConfiguration configuration;

		public HomeController(ILogger<HomeController> logger, Context context, DiscordSocketClient discord, IConfiguration configuration)
		{
			_logger = logger;
			this.context = context;
			this.discord = discord;
			this.configuration = configuration;
		}

		public IActionResult Index()
		{
			var model = new IndexModel();
			var session = HttpContext.Session.Get<SessionData>("Data");
			if (session?.ActiveGuild != null)
			{
				model.Guild = context.Guilds.FirstOrDefault(g => g.DiscordGuildId == session.ActiveGuild.Id);
				model.BotInGuild = discord.Guilds.Any(g => g.Id == session.ActiveGuild.Id);
			}
			model.ClientId = configuration["OAuth:Discord:ClientId"];

			return View(model);
		}
		
		[HttpGet]
		public IActionResult Login(string returnUrl = "/")
		{
			return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
		}

		[HttpGet("SwitchServer/{id}")]
		public IActionResult SwitchServer(ulong id)
		{
			var session = HttpContext.SessionData();
			session.ActiveGuild = session.Guilds.FirstOrDefault(g => g.Id == id);
			HttpContext.Session.Set<SessionData>("Data", session);
			return Redirect(Request.Headers["Referer"]);
		}

		[HttpGet("ActivateServer")]
		public async Task<IActionResult> ActivateServer()
		{
			var session = HttpContext.SessionData();
			if(session.ActiveGuild == null)
				return Problem("No guild selected");
			if (context.Guilds.Any(g => g.DiscordGuildId == session.ActiveGuild.Id))
				return Problem("Guild is already activated");

			Guild guild = new Guild()
			{
				DiscordGuildId = session.ActiveGuild.Id,
				GuildName = session.ActiveGuild.Name,
			};
			context.Guilds.Add(guild);
			await context.SaveChangesAsync();
			return Redirect(Request.Headers["Referer"]);
		}

		[HttpGet("Logout")]
		public async Task<IActionResult> Logout()
		{
			HttpContext.Session.Remove("Data");
			await HttpContext.SignOutAsync();
			return RedirectToAction("Index");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}