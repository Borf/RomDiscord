using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.GodRaffle;
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
        private readonly ModuleSettings moduleSettings;

        public ApiController(Context context, MvpHuntService mvpHuntService, DiscordSocketClient discord, ExchangeService exchangeService, ItemDb itemDb, ModuleSettings settings)
		{
			this.context = context;
			this.mvpHuntService = mvpHuntService;
			this.discord = discord;
			this.exchangeService = exchangeService;
			this.itemDb = itemDb;
            this.moduleSettings = settings;
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

        [HttpGet("Raffle/{guildid}")]
        public async Task<IActionResult> Raffle(int guildid)
        {
            var startDate = DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek - (int)DayOfWeek.Monday));
            var guild = context.Guilds.First(g => g.GuildId == guildid);
            SettingsModel settings = new SettingsModel(moduleSettings, guild);

            EmbedBuilder eb = new EmbedBuilder()
                .WithTitle("God Equip")
                .WithImageUrl("https://cdn.discordapp.com/attachments/819834309489590322/917802629155930152/GodRaffleFooter.png")
                .WithColor(9021952)
                .WithDescription("This is the god equipment for week of " + startDate.Date.ToString("D"))
                .WithFooter(new EmbedFooterBuilder().WithText("").WithIconUrl("https://cdn.discordapp.com/emojis/736643099274641419.png"));
            var currentDate = startDate;
            int c = 0;
            int rollAmountIndex = 0;
            int i = 0;
            while (i < 7)
            {
                if (settings.DaysEnabled.Contains(i))
                {
                    int len = settings.RollLengths[rollAmountIndex % settings.RollLengths.Count];
                    rollAmountIndex++;

                    string val = "";
                    var rolls = await context.GodEquipRolls.Where(r => r.GodEquip.Guild == guild && r.Date == DateOnly.FromDateTime(currentDate)).Include(r => r.GodEquip.GodEquip).ToListAsync();

                    var nextData = currentDate.AddDays(len);

                    if (DateTime.Now >= currentDate && DateTime.Now < nextData)
                    {
                        string lastName = "";
                        List<object> newRolls = new();
                        foreach (var roll in rolls)
                        {
                            var ign = context.Members.FirstOrDefault(m => m.Guild == guild && m.DiscordId == roll.UserId)?.Name;

                            newRolls.Add(new
                            {
                                Equip = roll.GodEquip.GodEquip.Name,
                                Level = roll.GodEquip.Level,
                                DiscordId = roll.UserId.ToString(),
                                Ign = ign
                            });
                        }
                        return Ok(new { RollId = rolls.First().GodEquipRollId, Rolls = newRolls });
                    }
                    currentDate = currentDate.AddDays(len);
                    i += len;
                }
                else
                {
                    currentDate = currentDate.AddDays(1);
                    i++;
                }
            }
            //var startDate = DateTime.Now.AddDays(-((int)startDate.DayOfWeek - (int)DayOfWeek.Monday));

            //var rolls = await context.GodEquipRolls.Where(r => r.GodEquip.Guild.GuildId == guildid && r.Date == DateOnly.FromDateTime(startDate)).Include(r => r.GodEquip.GodEquip).ToListAsync();
            return Ok("");
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
