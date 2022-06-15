using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.Events;
using RomDiscord.Services;
using RomDiscord.Util;

namespace RomDiscord.Controllers
{
	[Route("[controller]")]
	[Controller]
	public class EventsController : Controller
	{
		private readonly ModuleSettings settings;
		private readonly DiscordSocketClient discord;
		private readonly Context context;
		private readonly IWebHostEnvironment hostingEnvironment;
		private readonly EventService eventService;

		public EventsController(ModuleSettings settings, DiscordSocketClient discord, Context context, IWebHostEnvironment hostingEnvironment, EventService eventService)
		{
			this.settings = settings;
			this.discord = discord;
			this.context = context;
			this.hostingEnvironment = hostingEnvironment;
			this.eventService = eventService;
		}


		[HttpGet]
		[HttpGet("{year}/{month}")]
		public async Task<IActionResult> Index(int year = 0, int month = 0)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			if (year == 0)
				year = DateTime.Now.Year;
			if (month == 0)
				month = DateTime.Now.Month;

			var model = new RomDiscord.Models.Pages.Events.Index()
			{
				Year = year,
				Month = month,
				Events = await context.Events.Where(e => e.Guild == guild && (e.Repeats || (e.When.Month == month && e.When.Year == year))).ToListAsync(),
				Settings = new SettingsModel(settings, guild),
				Channels = discord.Guilds.First(g => g.Id == guild.DiscordGuildId).TextChannels,
			};

			return View(model);
		}

		[HttpGet("New/{year}/{month}/{day}")]
		public IActionResult New(int year, int month, int day)
		{
			var date = new DateTime(year, month, day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
			return View(date);
		}

		[HttpPost("New")]
		public async Task<IActionResult> NewEvent([FromForm] NewEventPost data)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var e = new Event()
			{
				Guild = guild,
				Name = data.Name,
				Description = data.Description,
				When = data.When,
				Repeats = data.Repeats,
				Length = new TimeSpan(data.LengthHours, data.LengthMinutes, 0),
				Where = data.Where,
				DiscordGuildEvent = data.DiscordEvent
			};
			if (data.Repeats)
				e.RepeatTime = new TimeSpan(data.RepeatDay ?? 0, data.RepeatHours ?? 0, 0, 0);
			context.Events.Add(e);
			await context.SaveChangesAsync();

			if (e.DiscordGuildEvent)
				await eventService.CreateDiscordGuildEvent(e.EventId);

			return RedirectToAction("Edit", new { eventId = e.EventId });
		}

		[HttpGet("Edit/{eventId}")]
		public async Task<IActionResult> Edit(int eventId)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var channels = discord.Guilds.First(g => g.Id == guild.DiscordGuildId).TextChannels;

			var e = await context.Events.FirstAsync(e => e.EventId == eventId);
			var s = new SettingsModel(settings, guild);

			var model = new RomDiscord.Models.Pages.Events.EditModel()
			{
				Event = e,
				Channel = channels.FirstOrDefault(c => c.Id == s.Channel),
				Guild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId)
			};

			return View(model);
		}
		[HttpPost("Edit/{id}")]
		public async Task<IActionResult> Edit(int id, [FromForm] NewEventPost data)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");

			var e = context.Events.First(e => e.EventId == id);
			e.Name = data.Name;
			e.Description = data.Description;
			e.When = data.When;
			e.Repeats = data.Repeats;
			e.Where = data.Where ?? "";
			e.Length = new TimeSpan(data.LengthHours, data.LengthMinutes, 0);
			if (e.Repeats)
				e.RepeatTime = new TimeSpan(data.RepeatDay ?? 0, data.RepeatHours ?? 0, 0, 0);
			if (data.Image != null)
			{
				e.HasImage = true;
				string outFile = Path.Combine(hostingEnvironment.WebRootPath, "img", "Events", e.EventId + ".png");
				using (var stream = System.IO.File.Create(outFile))
					await data.Image.CopyToAsync(stream);
			}

			await context.SaveChangesAsync();


			if (!e.DiscordGuildEvent && data.DiscordEvent && e.DiscordEventId == 0)
			{
				await eventService.CreateDiscordGuildEvent(id);
				e = context.Events.First(e => e.EventId == id);
			}
			else if (e.DiscordGuildEvent && !data.DiscordEvent && e.DiscordEventId != 0)
				await eventService.DeleteDiscordGuildEvent(id);
			else if(e.DiscordGuildEvent && data.DiscordEvent && e.DiscordEventId != 0)
				await eventService.UpdateDiscordGuildEvent(id);

			e.DiscordGuildEvent = data.DiscordEvent;
			await context.SaveChangesAsync();

			return Redirect(Request.Headers["Referer"]);
		}

		[HttpPost("ChangeSettings")]
		public async Task<IActionResult> ChangeSettings([FromForm] SettingsModel data)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");

			await settings.Set(guild, "events", "enabled", data.Enabled + "");
			await settings.Set(guild, "events", "channel", data.Channel + "");
			return Redirect(Request.Headers["Referer"]);
		}

		[HttpPost("CreateMessage/{id}")]
		public async Task<IActionResult> CreateMessage(int id)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var channels = discord.Guilds.First(g => g.Id == guild.DiscordGuildId).TextChannels;

			var e = await context.Events.FirstAsync(e => e.EventId == id);
			var s = new SettingsModel(settings, guild);

			var channel = channels.First(c => c.Id == s.Channel);

			var eb = EventService.BuildEmbed(e);

			var msg = await channel.SendMessageAsync("", false, eb);
			await msg.AddReactionAsync(new Emoji("⏰"));
			e.DiscordMessageId = msg.Id;
			await context.SaveChangesAsync();

			return Redirect(Request.Headers["Referer"]);
		}

		[HttpPost("UpdateMessage/{id}")]
		public async Task<IActionResult> UpdateMessage(int id)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			await eventService.UpdateEventPost(id);
			return Redirect(Request.Headers["Referer"]);
		}


	}
}