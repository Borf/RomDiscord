using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.GodRaffle;

namespace RomDiscord.Services
{
	public class GodEquipRaffle
	{
		private readonly Context context;
		private readonly ModuleSettings moduleSettings;
		private readonly DiscordSocketClient discord;

		public GodEquipRaffle(Context context, ModuleSettings moduleSettings, DiscordSocketClient discord)
		{
			this.context = context;
			this.moduleSettings = moduleSettings;
			this.discord = discord;
		}


		public async Task Initialize()
		{
			try
			{
				var guilds = context.Guilds.ToList();
				foreach (var guild in guilds)
				{
					if (moduleSettings.GetBool(guild, "godraffle", "enabled", false))
					{
						var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
						var channel = dcGuild.TextChannels.First(c => c.Id == moduleSettings.GetUlong(guild, "godraffle", "channel"));
						if (channel != null)
						{
							var lastMessages = await channel.GetMessagesAsync(100).FlattenAsync();

							foreach (var msg in lastMessages)
							{
								//Console.WriteLine(msg.Author.Username + "> " + msg.Content);
							}
							//Console.WriteLine("Initializing for guild " + guild.GuildName);
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}


		public async Task RaffleWeek(DateTime startDate, SocketGuild dcGuild)
		{
			startDate = startDate.AddDays(-((int)startDate.DayOfWeek - (int)DayOfWeek.Monday));
			var guild = context.Guilds.First(g => g.DiscordGuildId == dcGuild.Id);
			if (!moduleSettings.GetBool(guild, "godraffle", "enabled", false))
				return;
			var ges = await context.GodEquipGuild
				.Where(ge => ge.Guild == guild)
				.Include(ge => ge.GodEquip)
				.ToListAsync();
			var users = dcGuild.Users;
			Random r = new Random();
			SettingsModel settings = new SettingsModel(moduleSettings, guild);

			var currentDate = startDate;
			for (int di = 0; di < 7; di++)
			{
				foreach (var ge in ges)
					context.GodEquipRolls.RemoveRange(context.GodEquipRolls.Where(r => r.Date == DateOnly.FromDateTime(currentDate) && r.GodEquip == ge));
				currentDate = currentDate.AddDays(1);
			}
			await context.SaveChangesAsync();

			foreach (var ge in ges.OrderByDescending(ge => ge.GodEquip.Type).ThenBy(ge => users.Count(u => u.Roles.Any(r => r.Id == ge.DiscordRoleId) && u.Roles.Any(r => r.Id == 819475701232959528))).ToList())
			{
				for (int c = 0; c < ge.Amount; c++)
				{
					int rollAmountIndex = 0;
					currentDate = startDate;
					int dayIndex = 0;
					while (dayIndex < 7)
					{
						if (settings.DaysEnabled.Contains(dayIndex))
						{
							ulong roleId = ge.DiscordRoleId;
							var roleUsers = users.Where(u => u.Roles.Any(r => r.Id == roleId) && u.Roles.Any(r => r.Id == 819475701232959528)).ToList();
							if (roleUsers.Count == 0)
							{
								dayIndex = 7;
								break;
							}
							if (roleUsers.Count > 0)
							{
								var priorities = new Dictionary<SocketGuildUser, float>();
								foreach (var user in roleUsers)
								{
									float priority = 1;
									//add priority based on history
									var lastRoll = context.GodEquipRolls.Where(roll => roll.UserId == user.Id && roll.GodEquip == ge && roll.Date < DateOnly.FromDateTime(currentDate)).OrderByDescending(roll => roll.Date).FirstOrDefault();
									if (lastRoll == null)
										priority += settings.MaxTimeFactor;
									else
										priority += Math.Clamp((float)Math.Floor((currentDate - lastRoll.Date.ToDateTime(TimeOnly.FromDateTime(currentDate))).TotalDays) / settings.TimeFactor, 0.0f, settings.MaxTimeFactor);
									//TODO: softcode this role ID, add support for multiple
									if (user.Roles.Any(r => r.Id == settings.DonateRole))
										priority *= 1.5f;

									//lower chance if already have a god equip
									if (context.GodEquipRolls.Any(roll => roll.UserId == user.Id && roll.Date == DateOnly.FromDateTime(currentDate)))
										priority *= 0.001f;

									priorities[user] = priority;
								}
								double total = priorities.Sum(kv => kv.Value);
								double randomRoll = r.NextDouble() * total;
								int pick = 0;
								foreach (var user in roleUsers)
								{
									randomRoll -= priorities[user];
									if (randomRoll < 0)
										break;
									pick++;
								}

								for (int i = 0; i < settings.RollLengths[rollAmountIndex% settings.RollLengths.Count]; i++)
								{
									var roll = new GodEquipRoll()
									{
										Date = DateOnly.FromDateTime(currentDate),
										GodEquip = ge,
										UserId = roleUsers[pick].Id,
										Name = roleUsers[pick].Username + "#" + roleUsers[pick].Discriminator
									};
									context.GodEquipRolls.Add(roll);
									currentDate = currentDate.AddDays(1);
									dayIndex++;
									if (!settings.DaysEnabled.Contains(dayIndex) && dayIndex < 7)
									{
										currentDate = currentDate.AddDays(1);
										dayIndex++;
									}
								}
								rollAmountIndex++;
								await context.SaveChangesAsync();

							}
						}
					}
				}
			}
			await context.SaveChangesAsync();

			//build message
			{
//				var channel = dcGuild.TextChannels.First(c => c.Id == moduleSettings.GetUlong(guild, "godraffle", "channel"));
//				await channel.SendMessageAsync(null, false, await BuildEmbed(startDate, dcGuild, settings.Emoji));

			}
		}


		public async Task<Embed> BuildEmbed(DateTime startDate, SocketGuild dcGuild, bool emoji)
		{
			startDate = startDate.AddDays(-((int)startDate.DayOfWeek - (int)DayOfWeek.Monday));
			var guild = context.Guilds.First(g => g.DiscordGuildId == dcGuild.Id);
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
			while(i < 7)
			{
				if (settings.DaysEnabled.Contains(i))
				{
					int len = settings.RollLengths[rollAmountIndex % settings.RollLengths.Count];
					rollAmountIndex++;

					string val = "";
					var rolls = await context.GodEquipRolls.Where(r => r.GodEquip.Guild == guild && r.Date == DateOnly.FromDateTime(currentDate)).Include(r => r.GodEquip.GodEquip).ToListAsync();
					string lastName = "";
					foreach (var roll in rolls)
					{
						if (emoji)
						{
							if (roll.GodEquip.Emoji != "")
							{
								var e = dcGuild.Emotes.FirstOrDefault(e => e.Name == roll.GodEquip.Emoji);
								if (e != null)
									val += $"<:{e.Name}:{e.Id}> <@{roll.UserId}>\n";
								else
									val += $"{roll.GodEquip.GodEquip.Name} <@{roll.UserId}>\n";
							}
							else
								val += $"{roll.GodEquip.GodEquip.Name} <@{roll.UserId}>\n";
						}
						else
						{
							if (lastName != roll.GodEquip.GodEquip.Name)
								val += $"**{roll.GodEquip.GodEquip.Name}**\n";
							val += $"<@{roll.UserId}>\n";
							lastName = roll.GodEquip.GodEquip.Name;
						}
					}

					if (val != "")
					{
						string name = "**" + currentDate.DayOfWeek + "** " + currentDate.Day + "-" + currentDate.Month;
						var nextData = currentDate.AddDays(len);
						if (len > 1)
						{
							name = "**" + currentDate.DayOfWeek + "-" + nextData.DayOfWeek + "** " + currentDate.Day + "-" + currentDate.Month + " to " + nextData.Day + "-" + nextData.Month;
						}
						eb.AddField(new EmbedFieldBuilder()
							.WithName(name)
							.WithIsInline(true)
							.WithValue(val));
						c++;
						if (c % 2 == 1)
							eb.AddField(new EmbedFieldBuilder()
								.WithName("\u200B")
								.WithIsInline(true)
								.WithValue("\u200B"));
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
			return eb.Build();
		}


		public List<RomDiscord.Services.TaskScheduler.ScheduledTask> GetTasks()
		{
			DateTime nextSunday = DateTime.Now.AddDays((7 - (int)DateTime.Now.DayOfWeek) % 7).Date;
			nextSunday = nextSunday.AddHours(18);
			if (nextSunday < DateTime.Now)
				nextSunday = nextSunday.AddDays(7);

			return new List<RomDiscord.Services.TaskScheduler.ScheduledTask>()
			{
				new RomDiscord.Services.TaskScheduler.ScheduledTask()
				{
					Name = "God Equip Scheduler",
					NextRun = nextSunday, //next sunday
					TimeSpan = TimeSpan.FromDays(7), //every week
					Action = async (services) =>
					{
						Console.WriteLine("Running god raffle task!");
						using var scope = services.CreateScope();
						using var context = scope.ServiceProvider.GetRequiredService<Context>();
						var moduleSettings = scope.ServiceProvider.GetRequiredService<ModuleSettings>();
						var discord = services.GetRequiredService<DiscordSocketClient>();
						var godRaffle = scope.ServiceProvider.GetRequiredService<GodEquipRaffle>();
						var nextWeek = DateTime.Now.AddDays(2);
						var guilds = context.Guilds.ToList();
						foreach (var guild in guilds)
						{
							if (moduleSettings.GetBool(guild, "godraffle", "enabled", false))
							{
								var dcGuild = discord.Guilds.First(g => g.Id == guild.DiscordGuildId);
								await godRaffle.RaffleWeek(nextWeek, dcGuild);
								var channel = dcGuild.TextChannels.First(c => c.Id == moduleSettings.GetUlong(guild, "godraffle", "channel"));
								await channel.SendMessageAsync(null, false, await godRaffle.BuildEmbed(nextWeek, dcGuild, moduleSettings.GetBool(guild, "godraffle", "emoji", true)));
							}
						}
					}
				}
			};
		}
	}
}
