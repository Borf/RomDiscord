using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RomDiscord.Models.Db;
using RomDiscord.Models.Rest.Api;
using RomDiscord.Services;

namespace RomDiscord.Controllers
{
	[Route("api")]
	[ApiController]
	public class ApiController : ControllerBase
	{
		private readonly Context context;
		private readonly MvpHuntService mvpHuntService;
		private readonly DiscordSocketClient discord;
		private readonly ExchangeService exchangeService;
		private readonly ItemDb itemDb;

		public ApiController(Context context, MvpHuntService mvpHuntService, DiscordSocketClient discord, ExchangeService exchangeService, ItemDb itemDb)
		{
			this.context = context;
			this.mvpHuntService = mvpHuntService;
			this.discord = discord;
			this.exchangeService = exchangeService;
			this.itemDb = itemDb;
		}

		[HttpGet("test")]
		public IActionResult Test()
		{
			return Ok("ok!");
		}

		[HttpPost("MvpScan")]
		public async Task<IActionResult> MvpScan([FromBody] List<MvpScan> data)
		{
			string server = "EU";
			List<string> channels = new List<string>();

			foreach (var scan in data)
			{
				var s = context.MvpScans.FirstOrDefault(s => s.ScanTime == scan.ScanTime && s.MvpId == scan.MvpId && s.Channel == scan.Channel);
				if (s == null)
				{
					s = scan;
					context.MvpScans.Add(s);
				}
				else
				{
					s.CharName = scan.CharName;
					s.AliveTime = scan.AliveTime;
				}
				if(!channels.Contains(s.Channel))
					channels.Add(s.Channel);
			}
			await context.SaveChangesAsync();

			await mvpHuntService.UpdateChannel(server, channels);
			return Ok("Ok");
		}


		[HttpPost("OccupationScan")]
		public async Task<IActionResult> MvpScan([FromBody] List<OccupationScan> data)
		{
			string server = "EU";
			List<string> channels = new List<string>();

			foreach (var scan in data)
			{
				var s = context.OccupationScans.FirstOrDefault(s => s.ScanTime == scan.ScanTime && s.CastleId == scan.CastleId && s.Channel == scan.Channel);
				if (s == null)
				{
					s = scan;
					context.OccupationScans.Add(s);
				}
				else
				{
					s.GuildName = scan.GuildName;
					s.LeaderName = scan.LeaderName;
					s.MemberCount = scan.MemberCount;
				}
				if (!channels.Contains(s.Channel))
					channels.Add(s.Channel);
			}
			await context.SaveChangesAsync();
			return Ok("Ok");
		}


		[HttpPost("ExchangeScan")]
		public async Task<IActionResult> ExchangeScan([FromBody] List<Models.Rest.Api.ExchangeScan> data)
		{
			foreach (var scan in data)
			{
				var lastScan = context.ExchangeScans.FirstOrDefault(s => s.ItemId == scan.itemId);
				if (lastScan == null)
				{
					lastScan = new Models.Db.ExchangeScan()
					{
						ItemId = scan.itemId
					};
					context.ExchangeScans.Add(lastScan);
				}
				bool changed = false;
				if (lastScan.Price != scan.price || lastScan.Amount != scan.amount)
				{
					Console.WriteLine($"Change for {itemDb[lastScan.ItemId].NameZh}, price from {lastScan.Price} to {scan.price}, amount from {lastScan.Amount} to {scan.amount}");
					changed = true;
				}
				lastScan.Price = scan.price;
				lastScan.Amount = scan.amount;
				if (changed)
					await exchangeService.Update(lastScan);
				await exchangeService.Status(lastScan);
			}
			await context.SaveChangesAsync();
			return Ok("Ok");
		}


		[HttpGet("Items")]
		public async Task<IActionResult> Search([FromQuery] string q)
		{
			if (q.StartsWith("[") && q.Contains("]"))
				q = q.Substring(q.IndexOf("]")+1).Trim();

			return Ok(
				itemDb.db.Values.Where(i => i.NameZh.ToLower().Contains(q.ToLower()) || i.id.ToString().Contains(q))
					.Select(i => new { value = i.id, text = $"[{i.id}] {i.NameZh}" })
					.Take(20)
					.ToList());
		}
	}
}
