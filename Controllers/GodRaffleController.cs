using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.GodRaffle;
using RomDiscord.Services;
using RomDiscord.Util;
using System.Globalization;

namespace RomDiscord.Controllers
{
	[Route("[controller]")]
	[Controller]
	public class GodRaffleController : Controller
	{
		private readonly Context context;
		private readonly DiscordSocketClient discord;
		private readonly ModuleSettings moduleSettings;

		public GodRaffleController(Context context, DiscordSocketClient discord, ModuleSettings moduleSettings)
		{
			this.context = context;
			this.discord = discord;
			this.moduleSettings = moduleSettings;
		}

		public async Task<IActionResult> Index()
		{
			ulong guildId = HttpContext.SessionData().ActiveGuild?.Id ?? 0;
			var model = new RomDiscord.Models.Pages.GodRaffle.Index()
			{
				GodEquip = await context.GodEquips.ToListAsync(),
				GodEquipGuild = await context
				.GodEquipGuild
					.Include(g => g.Guild)
					.Include(g => g.GodEquip)
					.Where(gg => gg.Guild.DiscordGuildId == guildId)
					.OrderBy(g => g.GodEquip.GodEquipId)
						.ThenBy(g => g.Level)
					.ToListAsync(),
				Roles = discord.Guilds.First(g => g.Id == guildId).Roles.OrderBy(r => r.Position).ToList(),
				Settings = new SettingsModel(moduleSettings, context.Guilds.First(g => g.DiscordGuildId == guildId)),
				Channels = discord.Guilds.First(g => g.Id == guildId).Channels
			};

			return View(model);
		}

		[HttpGet("CalendarMonth/{year?}/{month?}")]
		public async Task<IActionResult> CalendarMonth(int year = 0, int month = 0)
		{
			var session = HttpContext.SessionData();
			if (session.ActiveGuild == null)
				return Redirect("/");

			if (year == 0)
				year = DateTime.Now.Year;
			if (month == 0)
				month = DateTime.Now.Month;
			var days = new List<CalendarMonth.Day>();
			for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
			{
				days.Add(new Models.Pages.GodRaffle.CalendarMonth.Day() 
				{ 
					Date = i, 
					DayOfWeek = new DateTime(year, month, i).DayOfWeek,
					rolls = await context.GodEquipRolls
						.Include(r => r.GodEquip.Guild)
						.Include(r => r.GodEquip.GodEquip)
						.Where(r => r.GodEquip.Guild.DiscordGuildId == session.ActiveGuild.Id)
						.Where(r => r.Date == new DateOnly(year, month, i))
						.ToListAsync()
				});
			}
			var model = new RomDiscord.Models.Pages.GodRaffle.CalendarMonth()
			{
				Year = year,
				Month = month,
				Days = days
			};
			return View(model);
		}

		[HttpGet("CalendarWeek/{year?}/{week?}")]
		public async Task<IActionResult> CalendarWeek(int year = 0, int week = -1)
		{
			var session = HttpContext.SessionData();
			if (session.ActiveGuild == null)
				return Redirect("/");

			if (year == 0)
				year = DateTime.Now.Year;
			if (week == -1)
				week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

			DateTime currentDay = new DateTime(year, 1, 1);
			currentDay = currentDay.AddDays(week * 7);
			currentDay = currentDay.AddDays(-((int)currentDay.DayOfWeek - (int)DayOfWeek.Monday));
			while (CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(currentDay, CalendarWeekRule.FirstDay, DayOfWeek.Monday) < week)
				currentDay = currentDay.AddDays(7);
			while (CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(currentDay, CalendarWeekRule.FirstDay, DayOfWeek.Monday) > week)
				currentDay = currentDay.AddDays(-7);



			var days = new List<CalendarWeek.Day>();
			for (int i = 0; i < 7; i++)
			{
				days.Add(new Models.Pages.GodRaffle.CalendarWeek.Day()
				{
					Date = currentDay,
					DayOfWeek = (DayOfWeek)i,
					rolls = await context.GodEquipRolls
						.Include(r => r.GodEquip.Guild)
						.Include(r => r.GodEquip.GodEquip)
						.Where(r => r.GodEquip.Guild.DiscordGuildId == session.ActiveGuild.Id)
						.Where(r => r.Date == DateOnly.FromDateTime(currentDay))
						.ToListAsync()
				});
				currentDay = currentDay.AddDays(1);
			}
			var model = new RomDiscord.Models.Pages.GodRaffle.CalendarWeek()
			{
				Year = year,
				Week = week,
				Days = days
			};
			return View(model);
		}


		[HttpPost("AddNew")]
		public async Task<IActionResult> AddNew([FromForm]AddNewModel data)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			context.GodEquipGuild.Add(new GodEquipGuildBinding()
			{
				GodEquip = await context.GodEquips.FirstAsync(g => g.GodEquipId == data.GodEquip),
				Amount = data.Amount,
				Level = data.Level,
				DiscordRoleId = data.Role,
				Guild = guild,
				Emoji = "",
			}) ;
			await context.SaveChangesAsync();
			return Redirect(Request.Headers["Referer"]);
		}
		[HttpPost("ChangeSettings")]
		public async Task<IActionResult> ChangeSettings([FromForm]SettingsModel data)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");

			await moduleSettings.Set(guild, "godraffle", "enabled", data.Enabled + "");
			await moduleSettings.Set(guild, "godraffle", "timefactor", data.TimeFactor + "");
			await moduleSettings.Set(guild, "godraffle", "maxtimefactor", data.MaxTimeFactor + "");
			await moduleSettings.Set(guild, "godraffle", "guildmemberrole", data.GuildMemberRole + "");
			await moduleSettings.Set(guild, "godraffle", "donaterole", data.DonateRole + "");
			await moduleSettings.Set(guild, "godraffle", "channel", data.Channel + "");
			await moduleSettings.Set(guild, "godraffle", "daysenabled", string.Join(",", data.DaysEnabled));
			await moduleSettings.Set(guild, "godraffle", "rolllengths", string.Join(",", data.RollLengths));
			return Redirect(Request.Headers["Referer"]);
		}

		[HttpPost("Change/{id}/{newRoleId}")]
		public async Task<IActionResult> Change(int id, ulong newRoleId)
		{
			var roleBind = context.GodEquipGuild.First(geg => geg.GodEquipGuildBindingId == id);
			roleBind.DiscordRoleId = newRoleId;
			await context.SaveChangesAsync();
			return Ok("ok");
		}
		[HttpPost("ChangeEmo/{id}/{value}")]
		public async Task<IActionResult> ChangeEmo(int id, string value)
		{
			var roleBind = context.GodEquipGuild.First(geg => geg.GodEquipGuildBindingId == id);
			roleBind.Emoji = value;
			await context.SaveChangesAsync();
			return Ok("ok");
		}

        [HttpPost("Increase/{id}")]
        public async Task<IActionResult> Increase(int id)
        {
            var roleBind = context.GodEquipGuild.First(geg => geg.GodEquipGuildBindingId == id);
            roleBind.Amount++;
            await context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"]);
        }
        [HttpPost("Decrease/{id}")]
        public async Task<IActionResult> Decrease(int id)
        {
            var roleBind = context.GodEquipGuild.First(geg => geg.GodEquipGuildBindingId == id);
            roleBind.Amount--;
			if (roleBind.Amount <= 0)
				context.GodEquipGuild.Remove(roleBind);
            await context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"]);
        }



    }
}
