﻿using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using RomDiscord.Util;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using RomDiscord.Models.Rest.Api;
using Discord.Webhook;
using Discord;

namespace RomDiscord.Controllers
{

	[Route("[controller]")]
	[Controller]
	public class GameChatController : Controller
	{
		private readonly ModuleSettings settings;
		private readonly DiscordSocketClient discord;
		private readonly Context context;
        private readonly ItemDb itemDb;

        public GameChatController(ModuleSettings settings, DiscordSocketClient discord, Context context, ItemDb itemDb)
        {
            this.settings = settings;
            this.discord = discord;
            this.context = context;
            this.itemDb = itemDb;
        }

        public IActionResult Index()
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
			return View(new Models.Pages.MvpHunt.Index()
			{
				Settings = settings,
				Guild = guild,
				Channels = dcGuild.TextChannels.OrderBy(c => c.Position).ToList()
			});
		}

        [HttpPost("Update")]
        public async Task<IActionResult> Update(string url, string admins)
        {
            var guild = this.Guild(context);
            if (guild == null)
                return RedirectToAction("Index", "Home");
            await settings.Set(guild, "GameChat", "url", url);
            await settings.Set(guild, "GameChat", "admins", admins);
            return RedirectToAction("Index");
        }

        [HttpPost("UpdateChannel/{dcChannel}")]
		public async Task<IActionResult> UpdateChannel(ulong dcChannel)
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");
			await settings.Set(guild, "GameChat", "channel", dcChannel + "");
			return Ok("Ok");
		}

        [HttpGet("ws/{guildid}")]
        public async Task Get(int guildid)
        {
            var guild = context.Guilds.First(g => g.GuildId == guildid);
            if (guild == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var channelId = settings.GetUlong(guild, "GameChat", "channel");
                var channel = discord.GetGuild(guild.DiscordGuildId).GetTextChannel(channelId);

                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket, guild, context, channel, settings, itemDb);
            }
            else
            {
                Console.WriteLine("Not a websocket connection...");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        static List<(int, WebSocket)> sockets = new List<(int, WebSocket)>();

        static async Task Echo(WebSocket webSocket, Guild guild, Context context, SocketTextChannel channel, ModuleSettings settings, ItemDb itemDb)
        {
            Console.WriteLine("Got a websocket!");
            WebSocketReceiveResult? result = null;
            var buffer = new byte[1024 * 4];
            sockets.Add((guild.GuildId, webSocket));
            while (true)
            {
                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.CloseStatus.HasValue)
                        break;
                    //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    var data = JsonSerializer.Deserialize<NewChatMessage>(System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count));
                    Console.WriteLine("Data: " + JsonSerializer.Serialize(data));
                    if (data.Type == NewChatMessage.MessageType.ChatBox)
                    {
                        var client = new HttpClient();
                        var SuccessWebHook = new
                        {
                            username = data.Name.Replace("\u0002", ""),
                            content = "ChatBoxCommand: `" + data.Message + "`",
                        };
                        var content = new StringContent(JsonSerializer.Serialize(SuccessWebHook), Encoding.UTF8, "application/json");
                        await client.PostAsync("https://discordapp.com/api/webhooks/1082048936660967444/60n3-ykh0SMXKcgDe_Vd3aOV1v2bu0sgX0rDs2KSAj8rm8Jc_9vPEPY9GOqOP_p8Likt", content);

                        bool isAdmin = settings.Get(guild, "GameChat", "admins", "").Split(",").Select(x => ulong.Parse(x)).Contains(data.UserId);
                        string msg = data.Message.Trim();
                        string api = settings.Get(guild, "GameChat", "api", "");

                        if (isAdmin)
                        {
                            if (msg.ToLower() == "return")
                            {
                                await webSocket.SendAsync(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Chat = "Chat", Message = "God weapons returning..." })), WebSocketMessageType.Text, true, CancellationToken.None);
                                await webSocket.SendAsync(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Chat = "Cmd", Message = "guild.returngodequip" })), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else
                                await webSocket.SendAsync(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Chat = "Chat", Message = "Sorry, contact your guild leader" })), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        else
                        {
                            await webSocket.SendAsync(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Chat = "Chat", Message = "Hello. How can I help?" })), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    else
                    {
                        using var wh = new DiscordWebhookClient("https://discordapp.com/api/webhooks/1082048936660967444/60n3-ykh0SMXKcgDe_Vd3aOV1v2bu0sgX0rDs2KSAj8rm8Jc_9vPEPY9GOqOP_p8Likt");

                        string msg = data.Message;
                        string user = data.Name.Replace("\u0002", "");
                        List<Embed>? embeds = null;

                        if(data.ItemInfos.Count > 0)
                        {
                            embeds = new List<Embed>();
                            foreach(var  item in data.ItemInfos)
                            {
                                var itemdbitem = itemDb.db[item.ItemId];
                                int index = msg.IndexOf("{il=Item}");
                                if (index >= 0)
                                    msg = msg[0..index] + "__" + itemdbitem.NameZh + "__" + msg[(index + 9)..];
                                var eb = new EmbedBuilder()
                                    .WithTitle(itemdbitem.NameZh)
                                    .AddField("Price", Util.Util.FormatPrice(item.Price) + "z");

                                if (item.RefineLevel != null)
                                    eb.AddField("Refine", "+" + item.RefineLevel);
                                if(item.Broken != null)
                                    eb.AddField("Broken", item.Broken.ToString());
                                if(item.Enchants != null)
                                {
                                    eb.AddField("Enchants", string.Join("\n", item.Enchants.Select(e => e.ToString())));
                                }

                                embeds.Add(eb.Build());
                            }
                        }

                        if(data.Photo == null)
                            await wh.SendMessageAsync(text: msg, embeds: embeds, username: user);
                        else
                        {
                            await wh.SendFileAsync(new FileAttachment(await new HttpClient().GetStreamAsync($"http://eu-cdn.ro.com/game/photo/72001/user/{data.Photo.Charid}/{data.Photo.SourceId}.png"), "image.png"), text: "Photo:", embeds: embeds, username: user);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }
            }
            sockets.Remove((guild.GuildId, webSocket));
            if (result != null)
                await webSocket.CloseAsync(result?.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result?.CloseStatusDescription, CancellationToken.None);
        }

        public static async Task DiscordMessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;
            if (arg.Channel.Id != 1082003559106752603)
                return;
            var ws = sockets.First(s => s.Item1 == 1).Item2;

            var msg = "";
            msg += arg.Content;
            if (msg.Length > 30)
                msg = msg[0..30];

            //await ws.SendAsync(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Chat = "Chat", Message=msg }), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
