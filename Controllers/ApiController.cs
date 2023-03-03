using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RomDiscord.Models.Db;
using RomDiscord.Models.Rest.Api;
using RomDiscord.Services;
using RomDiscord.Util;
using System;
using System.Text.Json;

namespace RomDiscord.Controllers
{
	[Route("api")]
	[ApiController]
	public class ApiController : Controller
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
					s.MvpScanId = 0;
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
//			string server = "EU";
			List<string> channels = new List<string>();

			foreach (var scan in data)
			{
				var s = context.OccupationScans.FirstOrDefault(s => s.ScanTime == scan.ScanTime && s.CastleId == scan.CastleId && s.Channel == scan.Channel);
				if (s == null)
				{
					s = scan;
					s.OccupationScanId = 0;
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
				if (scan.Entries.Count > 0)
					lastScan.LastPrice = scan.Entries[0].price;

				if (scan.Entries.Count > 0)
					lastScan.LastSeen = DateTime.Now;
//				await exchangeService.Status(scan);
			}
			await context.SaveChangesAsync();
			return Ok("Ok");
		}

        [HttpPost("NewExchangeScan")]
        public async Task<IActionResult> ExchangeScan([FromBody] Models.Rest.Api.NewExchangeScan data)
        {
			//Console.WriteLine(JsonSerializer.Serialize(data));
            await exchangeService.NewScanResult(data);
            return Ok("Ok");
        }
        [HttpPost("NewExchangeScanAllItems")]
        public async Task<IActionResult> NewExchangeScanAllItems(List<int> itemsScanned)
        {
			await exchangeService.NewScanItemList(itemsScanned);
            return Ok("Ok");
        }



        [HttpGet("Items")]
		public IActionResult Search([FromQuery] string q)
		{
			if (q.StartsWith("[") && q.Contains("]"))
				q = q.Substring(q.IndexOf("]")+1).Trim();

			return Ok(
				itemDb.db.Values.Where(i => i.NameZh.ToLower().Contains(q.ToLower()) || i.id.ToString().Contains(q))
					.Select(i => new { value = i.id, text = $"[{i.id}] {i.NameZh}" })
					.Take(20)
					.ToList());
		}


		[HttpGet("Channels")]
		public IActionResult SearchChannels([FromQuery] string q)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return Problem("Not authenticated");


			return Ok(
				discord.Guilds.First(g => g.Id == guild.DiscordGuildId).TextChannels.Where(c => c.Name.ToLower().Contains(q.ToLower())).OrderBy(c => c.Position)
					.Select(i => new { value = i.Id+"", text = i.Name })
					.Take(20)
					.ToList());
		}
	}
}
