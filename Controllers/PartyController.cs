using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.Party;
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

			var lastFewAttendance = await context.Attendance.Include(a => a.Members).ThenInclude(ma => ma.Member).OrderByDescending(a => a.Date).Take(7).OrderBy(a => a.Date).ToListAsync();
			var memberAttendance = context.Members.Where(m => m.Guild == guild).ToDictionary(keySelector: m => m, elementSelector : m => new List<bool>());
			foreach(var att in lastFewAttendance)
				foreach(var member in memberAttendance.Keys)
					memberAttendance[member].Add(att.Members.Any(ma => ma.Member == member));


			return View(new RomDiscord.Models.Pages.Party.Index()
			{
				LastAttendance = await context.Attendance.Include(a => a.Members).OrderByDescending(a => a.Date).FirstAsync(),
				Members = await context.Members.Where(m => m.Guild == guild).ToListAsync(),
				Parties = await context.Parties.Include(p => p.Members).Where(p => p.Guild == guild).ToListAsync(),
				Channels = discord.Guilds.First(g => g.Id == guild?.DiscordGuildId).Channels,
				ActiveChannel = settings.GetUlong(guild, "party", "channel"),
				MemberAttendance = memberAttendance
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
		public async Task<IActionResult> Publish([FromForm]PublishFormModel data)
		{
			var guild = this.Guild(context);
			if(guild == null)
				return RedirectToAction("Index", "Home");
			await settings.Set(guild, "party", "channel", data.Channel + "");
			var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
			var channel = dcGuild.TextChannels.First(c => c.Id == data.Channel);

			var eb = new EmbedBuilder();
			int partyIndex = 0;
			foreach(var party in context.Parties.Include(p => p.Members).Where(p => p.Guild == guild))
			{
				if (party.Members.Count == 0)
					continue;
				string text = "";
				if(party.Leader != null)
				{
					text += "`" + party.Leader.Name.PadRight(14) + "`";
					text += ":star:";
					foreach (var job in party.Leader.JobList)
					{
						var emoteId = settings.GetUlong(guild, "emoji", job + "");
						if (emoteId != 0)
						{
							var emote = dcGuild.Emotes.FirstOrDefault(e => e.Id == emoteId);
							if (emote != null)
								text += $"<:{emote.Name}:{emote.Id}>";
						}
					}
					text += string.Join("", Enumerable.Repeat(":black_small_square:", Math.Max(0, 3 - party.Leader.JobList.Count)));
					text += "\n";
				}
				foreach (var member in party.Members.Where(m => m != party.Leader))
				{
					text += "`" + member.Name.PadRight(14) + "`";
						text += ":black_small_square:";
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
					.WithValue(text);
				if (party.Role == Party.PartyRole.None)
					ef.WithName(party.Name);
				else
					ef.WithName(party.Name + " (" + party.Role + ")");

				eb.AddField(ef);
				if ((partyIndex++) % 2 == 1)
					eb.AddField(new EmbedFieldBuilder().WithName("\u200b").WithIsInline(true).WithValue("\u200b"));
			}

			if (data.Image != null)
				await channel.SendFileAsync(data.Image.OpenReadStream(), "Image.png", data.Description, false, eb.Build());
			else
				await channel.SendMessageAsync(data.Description, false, eb.Build());


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
							var member = context.Members.First(m => m.MemberId == data.GetProperty("Member").GetInt32());
							if (member.PartyId != null)
							{
								var oldParty = context.Parties.First(p => p.PartyId == member.PartyId);
								if (oldParty.Leader == member)
									oldParty.Leader = null;
							}
							member.PartyId = partyId;
							await context.SaveChangesAsync();
							await Broadcast(new { Action = "Move", Member = data.GetProperty("Member").GetInt32(), Party = data.GetProperty("Party").GetInt32()  }, guild, webSocket);
							Console.WriteLine("Moving");
							break;
						case "PartyName":
							context.Parties.First(p => p.PartyId == data.GetProperty("Party").GetInt32()).Name = data.GetProperty("Name").GetString() ?? "";
							await context.SaveChangesAsync();
							await Broadcast(new { Action = "PartyName", Party= data.GetProperty("Party").GetInt32(), Name = data.GetProperty("Name").GetString() }, guild, webSocket);
							break;
						case "Leader":
							context.Parties.First(p => p.PartyId == data.GetProperty("Party").GetInt32()).Leader = context.Members.First(m => m.MemberId == data.GetProperty("Member").GetInt32());
							await context.SaveChangesAsync();
							await Broadcast(new { Action = "Leader", Member = data.GetProperty("Member").GetInt32(), Party = data.GetProperty("Party").GetInt32() }, guild, webSocket);
							break;
						case "PartyRole":
							context.Parties.First(p => p.PartyId == data.GetProperty("Party").GetInt32()).Role = Enum.Parse<Party.PartyRole>(data.GetProperty("Role").GetString() ?? "");
							await context.SaveChangesAsync();
							await Broadcast(new { Action = "PartyRole", Party = data.GetProperty("Party").GetInt32(), Role = data.GetProperty("Role").GetString() }, guild, webSocket);
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
