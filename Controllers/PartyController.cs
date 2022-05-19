using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using RomDiscord.Util;
using System.Net.WebSockets;
using System.Text.Json;

namespace RomDiscord.Controllers
{
	[Route("[controller]")]
	[Controller]
	public class PartyController : Controller
	{
		private readonly Context context;
		private readonly DiscordSocketClient discord;
		private readonly ModuleSettings settings;

		public PartyController(Context context, DiscordSocketClient discord, ModuleSettings settings)
		{
			this.context = context;
			this.discord = discord;
			this.settings = settings;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var guild = this.Guild(context);
			if (guild == null)
				return RedirectToAction("Index", "Home");

			return View(new RomDiscord.Models.Pages.Party.Index()
			{
				Members = await context.Members.Where(m => m.Guild == guild).ToListAsync(),
				Parties = await context.Parties.Include(p => p.Members).Where(p => p.Guild == guild).ToListAsync(),
				Channels = discord.Guilds.First(g => g.Id == guild?.DiscordGuildId).Channels,
				ActiveChannel = settings.GetUlong(guild, "party", "channel")
			});
		}


		[HttpGet("ws")]
		public async Task Get()
		{
			var guild = this.Guild(context);
			if (guild == null)
			{
				HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}
			if (HttpContext.WebSockets.IsWebSocketRequest)
			{
				using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
				await Echo(webSocket, guild, context);
			}
			else
				HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
		}

		[HttpPost("Publish")]
		public async Task<IActionResult> Publish(ulong Channel)
		{
			var guild = this.Guild(context);
			if(guild == null)
				return RedirectToAction("Index", "Home");
			await settings.Set(guild, "party", "channel", Channel+"");
			var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
			var channel = dcGuild.TextChannels.First(c => c.Id == Channel);

			var eb = new EmbedBuilder();
//			int partyIndex = 0;
			foreach(var party in context.Parties.Include(p => p.Members).Where(p => p.Guild == guild))
			{
				if (party.Members.Count == 0)
					continue;
				string text = "";
				foreach (var member in party.Members)
				{
					text += "`" + member.Name.PadRight(14) + "`";
					foreach (var job in member.JobList)
					{
						var emoteId = settings.GetUlong(guild, "emoji", job+"");
						if (emoteId != 0)
						{
							var emote = dcGuild.Emotes.FirstOrDefault(e => e.Id == emoteId);
							if (emote != null)
								text += $"<:{emote.Name}:{emote.Id}>";
						}
					}
					text += string.Join("",Enumerable.Repeat(":black_small_square:", 3 -  member.JobList.Count));
					text += "\n";
				}
				text += "";

				var ef = new EmbedFieldBuilder()
					.WithIsInline(true)
					.WithName(party.Name)
					.WithValue(text);
				eb.AddField(ef);
				//if ((partyIndex++) % 2 == 1)
				//	eb.AddField(new EmbedFieldBuilder().WithName("\u200b").WithIsInline(true).WithValue("\u200b"));
			}


			await channel.SendMessageAsync("New Parties", false, eb.Build());


			return Ok("Ok");
		}



		static List<(int, WebSocket)> sockets = new List<(int, WebSocket)>();
		static async Task Echo(WebSocket webSocket, Guild guild, Context context)
		{
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
				
					JsonElement data = JsonSerializer.Deserialize<JsonElement>(System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count));
					string action = data.GetProperty("Action").GetString() ?? "";
					switch(action)
					{
						case "NewParty":
							var party = new Party()
							{
								Guild = guild,
								Name = data.GetProperty("Name").GetString() ?? ""
							};
							context.Parties.Add(party);
							await context.SaveChangesAsync();
							await Broadcast(new { Action= "NewParty", Id= party.PartyId, Name=party.Name }, guild);
							break;
						case "Move":
							int? partyId = data.GetProperty("Party").GetInt32();
							if (partyId == 0)
								partyId = null;
							context.Members.First(m => m.MemberId == data.GetProperty("Member").GetInt32()).PartyId = partyId;
							await context.SaveChangesAsync();
							await Broadcast(new { Action = "Move", Member = data.GetProperty("Member").GetInt32(), Party = data.GetProperty("Party").GetInt32()  }, guild, webSocket);
							Console.WriteLine("Moving");
							break;
						case "PartyName":
							context.Parties.First(p => p.PartyId == data.GetProperty("Party").GetInt32()).Name = data.GetProperty("Name").GetString() ?? "";
							await context.SaveChangesAsync();
							await Broadcast(new { Action = "PartyName", Party= data.GetProperty("Party").GetInt32(), Name = data.GetProperty("Name").GetString() }, guild, webSocket);
							break;
						default:
							Console.WriteLine("Unknown packet action: " + action);
							break;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					break;
				}
			}
			sockets.Remove((guild.GuildId, webSocket));
			if(result != null)
				await webSocket.CloseAsync(result?.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result?.CloseStatusDescription, CancellationToken.None);
		}

		static async Task Broadcast(dynamic packet, Guild guild, WebSocket? except = null)
		{
			foreach (var webSocket in sockets)
			{
				if(webSocket.Item1 == guild.GuildId && webSocket.Item2 != except)
					await webSocket.Item2.SendAsync(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}
	}

}
