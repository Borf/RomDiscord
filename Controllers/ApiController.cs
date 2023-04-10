using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
using RomDiscord.Models.Db;
using RomDiscord.Models.Rest.Api;
using RomDiscord.Services;
using RomDiscord.Util;
using System;
using System.Net.WebSockets;
using System.Text;
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

		[HttpPost("PvpRankings")]
		public IActionResult PvpRankings([FromBody] PvpData pvpData)
		{
			Console.WriteLine(JsonSerializer.Serialize(pvpData, new JsonSerializerOptions() { WriteIndented = true }));
			return Ok( "ok");
		}


		[HttpPost("searchchars/{q}")]
		public async Task<IActionResult> FriendSearchQuery(string q)
		{
			foreach(var socket in sockets)
			{
				tcs = new TaskCompletionSource<FriendFindData>();
				await socket.SendAsync(System.Text.Encoding.UTF8.GetBytes(q), WebSocketMessageType.Text, true, CancellationToken.None);
				var res = await tcs.Task;
				return Ok(res);
			}
			return Ok("Ok");
		}



        [HttpGet("friendsearch")]
        public async Task FriendSearch()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket, context);
            }
            else
            {
                Console.WriteLine("Not a websocket connection...");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        public static List<WebSocket> sockets = new List<WebSocket>();
        static async Task Echo(WebSocket webSocket, Context context)
        {
            Console.WriteLine("Got a websocket!");
            WebSocketReceiveResult? result = null;
            var buffer = new byte[1024 * 48];
            sockets.Add(webSocket);
            while (true)
            {
                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.CloseStatus.HasValue)
                        break;
                    //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    tcs.SetResult(JsonSerializer.Deserialize<FriendFindData>(System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }
            }
            sockets.Remove(webSocket);
            if (result != null)
                await webSocket.CloseAsync(result?.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result?.CloseStatusDescription, CancellationToken.None);
        }

		public static TaskCompletionSource<FriendFindData>? tcs;
    }
}
