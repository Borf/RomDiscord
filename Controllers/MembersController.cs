using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.Members;
using RomDiscord.Util;

namespace RomDiscord.Controllers
{
	[Route("[controller]")]
	[Controller]
	public class MembersController : Controller
	{
		private readonly Context context;
		private readonly DiscordSocketClient discord;

		public MembersController(Context context, DiscordSocketClient discord)
		{
			this.context = context;
			this.discord = discord;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var guild = this.Guild(context);
			if(guild == null)
				return RedirectToAction("Index", "Home");
			var model = new Models.Pages.Members.Index()
			{
				Members = await context.Members.Where(m => m.Guild == guild).ToListAsync(),
				DiscordMembers = discord.Guilds.First(g => g.Id == guild.DiscordGuildId).Users.ToList()
			};

			return View(model);
		}

		[HttpGet("Edit/{memberId}")]
		public async Task<IActionResult> Edit(int memberId)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var model = new Models.Pages.Members.EditModel()
			{
				Member = await context.Members.FirstAsync(m => m.MemberId == memberId),
				DiscordMembers = discord.Guilds.First(g => g.Id == guild.DiscordGuildId).Users.ToList()
			};
			return View(model);
		}

		[HttpGet("Stats")]
		public async Task<IActionResult> Stats()
		{
			var guild = this.Guild(context);
			return View(await context.Members.Where(m => m.Guild == guild).ToListAsync());
		}

		[HttpPost("AddMember")]
		public async Task<IActionResult> AddMember([FromForm]AddMemberModel data)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var dc = discord.Guilds.FirstOrDefault(g => g.Id == guild.DiscordGuildId);
			SocketGuildUser? dcUser = null;
			if(dc != null)
				dcUser = dc.Users.FirstOrDefault(u => u.Id == data.DiscordId);
			context.Members.Add(new Member()
			{
				Name = data.Name,
				Guild = guild,
				DiscordId = data.DiscordId,
				DiscordName = dcUser == null ? "" : (dcUser.Username + "#" + dcUser.Discriminator),
				AlternativeNames = "",
				Jobs = "",
				ShortNote = "",
				LongNote = "",
			});
			await context.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[HttpPost("Left/{memberId}")]
		public async Task<IActionResult> Left(int memberId, [FromForm] UpdateMemberModel data)
		{
			var member = context.Members.First(m => m.MemberId == memberId);
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");

			member.Active = false;

			await context.SaveChangesAsync();

			return RedirectToAction("Index");
		}


		[HttpPost("Update/{memberId}")]
		public async Task<IActionResult> UpdateMember(int memberId, [FromForm]UpdateMemberModel data)
		{
			var member = context.Members.First(m => m.MemberId == memberId);
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");

			member.Name = data.Name;
			member.ShortNote = data.ShortNote;
			member.LongNote = data.LongNote;
			member.JoinDate = data.JoinDate;
			member.AlternativeNames = data.AlternativeNames ?? "";
			//member.JobList = data.Jobs;
			if (data.Jobs != null)
				member.Jobs = String.Join(",", data.Jobs);

			member.DiscordId = data.DiscordId;
			var dc = discord.Guilds.FirstOrDefault(g => g.Id == guild.DiscordGuildId);
			SocketGuildUser? dcUser = null;
			if (dc != null)
				dcUser = dc.Users.FirstOrDefault(u => u.Id == data.DiscordId);
			if (dcUser != null)
				member.DiscordName = dcUser.Username + "#" + dcUser.Discriminator;

			await context.SaveChangesAsync();

			return RedirectToAction("Index");
		}
	}
}
