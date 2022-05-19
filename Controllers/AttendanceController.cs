using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.Attendance;
using RomDiscord.Services;
using RomDiscord.Util;

namespace RomDiscord.Controllers
{
	[Route("[controller]")]
	[Controller]
	public class AttendanceController : Controller
	{
		private readonly Context context;
		private readonly DiscordSocketClient discord;
		private readonly ModuleSettings moduleSettings;

		public AttendanceController(Context context, DiscordSocketClient discord, ModuleSettings moduleSettings)
		{
			this.context = context;
			this.discord = discord;
			this.moduleSettings = moduleSettings;
		}

		[HttpGet]
		[HttpGet("{year}/{month}")]
		public async Task<IActionResult> Index(int year = 0, int month = 0)
		{
			if (HttpContext.SessionData().ActiveGuild == null)
				return RedirectToAction("Index", "Home");
			var guild = this.Guild(context);
			if (year == 0)
				year = DateTime.Now.Year;
			if (month == 0)
				month = DateTime.Now.Month;

			var model = new RomDiscord.Models.Pages.Attendance.Index()
			{
				Year = year,
				Month = month,
				Attendance = await context.Attendance
					.Include(a => a.Members)
					.Where(a => a.Guild == guild && a.Date.Month == month && a.Date.Year == year)
					.ToDictionaryAsync(keySelector: a => a.Date.Day, elementSelector: a => a)
			};

			return View(model);
		}

		[HttpGet("Record/{year}/{month}/{day}")]
		public async Task<IActionResult> Record(int year, int month, int day)
		{
			var guild = this.Guild(context);
			return View(new RecordModel()
			{
				Year = year,
				Month = month,
				Day = day,
				AllMembers = await context.Members.Where(m => m.Guild == guild).OrderBy(m => m.Name).ToListAsync(),
				Members = (await context.Attendance
				.Include(a => a.Members)
				.FirstOrDefaultAsync(a => a.Guild == guild && a.Date.Year == year && a.Date.Month == month && a.Date.Day == day))
					?.Members
						.Select(m => m.Member).ToList() ?? new List<Member>()
			});
		}

		[HttpPost("Record/{year}/{month}/{day}")]
		public async Task<IActionResult> RecordPost(int year, int month, int day, [FromForm]List<int> Members)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var attendance = context.Attendance.FirstOrDefault(a => a.Date == new DateOnly(year, month, day));
			if (attendance == null)
			{
				attendance = new Attendance()
				{
					Date = new DateOnly(year, month, day),
					Guild = guild
				};
				context.Attendance.Add(attendance);
				await context.SaveChangesAsync();
			}
			context.AttendanceMembers.RemoveRange(context.AttendanceMembers.Where(am => am.Attendance == attendance));
			await context.SaveChangesAsync();
			foreach (var id in Members)
			{
				context.AttendanceMembers.Add(new AttendanceMember()
				{
					Attendance = attendance,
					Member = context.Members.First(m => m.MemberId == id && m.Guild == guild)
				});
			}
			await context.SaveChangesAsync();
			return RedirectToAction("Index", new {Year = year, Month = month});
		}
	}
}
