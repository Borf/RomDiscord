using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.Events;

namespace RomDiscord.Services
{
    public class EventService
    {
		private readonly Context context;
		private readonly DiscordSocketClient discord;
        private readonly ModuleSettings moduleSettings;
		private readonly IWebHostEnvironment hostingEnvironment;

		public EventService(Context context, DiscordSocketClient discord, ModuleSettings moduleSettings, IWebHostEnvironment hostingEnvironment)
		{
			this.context = context;
			this.discord = discord;
			this.moduleSettings = moduleSettings;
			this.hostingEnvironment = hostingEnvironment;
		}

		public List<RomDiscord.Services.TaskScheduler.ScheduledTask> GetTasks()
		{
			//make one task for the next scheduled evens
			var nextEvent = NextEvent(context);
			if (nextEvent == null)
				return new List<RomDiscord.Services.TaskScheduler.ScheduledTask>();
            Console.WriteLine("The first event is " + nextEvent.Name + " at " + nextEvent.NextWhen);
			var tasks = new List<RomDiscord.Services.TaskScheduler.ScheduledTask>()
			{
				new RomDiscord.Services.TaskScheduler.ScheduledTask()
				{
					Name = "EventStarter",
					NextRun = nextEvent.NextWhen,
					Action = StartEvents
				}
			};
			//make tasks for all events that need to be cleaned up (if the bot got restarted during an event)
			var runningEvents = context.Events.Include(e => e.Guild).Where(e => e.DiscordNotificationId != 0).ToList();
			foreach(var e in runningEvents)
            {
				if(!e.IsActive) // uhoh, this event ended but it has a notification
                {
					Console.WriteLine(DateTime.Now + "\tCleaning up notification for " + e.Name);
					var settings = new SettingsModel(moduleSettings, e.Guild);
					var channel = discord.Guilds.First(g => g.Id == e.Guild.DiscordGuildId).TextChannels.First(c => c.Id == settings.Channel);
					try
					{
						channel.DeleteMessageAsync(e.DiscordNotificationId).Wait();
					}
					catch (Exception) { }
					e.DiscordNotificationId = 0;
					context.SaveChangesAsync().Wait();
                } 
				else
                {
                    Console.WriteLine(DateTime.Now + "\tScheduling cleanup for task " + e.Name);
					tasks.Add(new RomDiscord.Services.TaskScheduler.ScheduledTask()
					{
						Name = "Finish Event " + e.Name,
						NextRun = e.CurrentWhen + e.Length,
						Action = FinishEvent(e.EventId)
					});
                }
            }
			return tasks;
		}


		public async Task UpdateEventPost(int eventId)
        {
			var e = await context.Events.Include(e => e.Guild).FirstAsync(e => e.EventId == eventId);
			var dcGuild = discord.Guilds.First(g => g.Id == e.Guild.DiscordGuildId);
			var channels = dcGuild.TextChannels;
			var s = new SettingsModel(moduleSettings, e.Guild);
			var channel = channels.First(c => c.Id == s.Channel);
			if (e.DiscordMessageId != 0)
			{
				try
				{
					var eb = EventService.BuildEmbed(e);
					var msg = await channel.ModifyMessageAsync(e.DiscordMessageId, m => m.Embed = eb);
					if (!msg.Reactions.Any(r => r.Key.Name == "⏰"))
						await msg.AddReactionAsync(new Emoji("⏰"));
				}catch(Exception)
				{ }
			}
		}
		public static Embed BuildEmbed(Event e)
		{
			DateTime nextOccurrance = e.When;
			while (e.Repeats && nextOccurrance < DateTime.Now)
				nextOccurrance += e.RepeatTime;

			EmbedBuilder eb = new EmbedBuilder()
				.WithTitle(e.Name)
				.WithDescription(e.Description)
				.AddField(new EmbedFieldBuilder()
					.WithName("When")
					.WithValue("<t:" + new DateTimeOffset(nextOccurrance).ToUnixTimeSeconds() + "> " + "<t:" + new DateTimeOffset(nextOccurrance).ToUnixTimeSeconds() + ":R>")
					)
				.AddField(new EmbedFieldBuilder()
					.WithName("Length")
					.WithValue(e.Length.TotalMinutes + " minutes")
					)
			;
			if (e.HasImage) //TODO: remove hardcoding
				eb.WithImageUrl("http://romdiscord.borf.nl/img/Events/" + e.EventId + ".png");

			if (e.Where != "")
				eb.AddField(new EmbedFieldBuilder()
					.WithName("Where")
					.WithValue(e.Where));

			return eb.Build();
		}

    

        public static async void StartEvents(IServiceProvider services)
        {
            Console.WriteLine(DateTime.Now + "\tAn event is starting!");
			using var scope = services.CreateScope();
			using var context = scope.ServiceProvider.GetRequiredService<Context>();
			var scheduler = (RomDiscord.Services.TaskScheduler)services.GetServices<IHostedService>().First(s => (s as RomDiscord.Services.TaskScheduler) != null);
			var moduleSettings = scope.ServiceProvider.GetRequiredService<ModuleSettings>();
			var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
			var nextEvent = NextEvent(context);
			if (nextEvent != null)
			{
                Console.WriteLine(DateTime.Now + "\tScheduling new task for " + nextEvent.Name + "(" + nextEvent.NextWhen + ")");
				scheduler.ScheduleTask(new RomDiscord.Services.TaskScheduler.ScheduledTask()
				{
					Name = "EventStarter",
					NextRun = nextEvent.NextWhen,
					Action = StartEvents
				});
			}

			var eventsToActivate = await context.Events
				.Include(e => e.Guild)
				.Where(e => e.DiscordNotificationId == 0 && ((!e.Repeats && e.When >= DateTime.Now) || (e.Repeats)))
				.ToListAsync();

			foreach(var e in eventsToActivate.Where(e => e.IsActive))
            {
				var settings = new SettingsModel(moduleSettings, e.Guild);
				Console.WriteLine(DateTime.Now + "\tActivating event " + e.Name + ", notification: " + e.DiscordNotificationId);
				var discord = services.GetRequiredService<DiscordSocketClient>();
				var channel = discord.Guilds.First(g => g.Id == e.Guild.DiscordGuildId).TextChannels.First(c => c.Id == settings.Channel);

				string msgText = "Event " + e.Name + " is starting! ends at " + (e.CurrentWhen + e.Length) + "\n";
				if (e.DiscordMessageId != 0)
				{
					var regMsg = await channel.GetMessageAsync(e.DiscordMessageId);
					var users = await regMsg.GetReactionUsersAsync(new Emoji("⏰"), 400).FlattenAsync();
					msgText += string.Join(", ", users.Where(u => !u.IsBot).Select(u => u.Mention));
				}
				var msg = await channel.SendMessageAsync(msgText);
				e.DiscordNotificationId = msg.Id;
				Console.WriteLine(DateTime.Now + "\tScheduling finish for " + (e.CurrentWhen + e.Length)); ;
				scheduler.ScheduleTask(new RomDiscord.Services.TaskScheduler.ScheduledTask()
				{
					Name = "Finish Event " + e.Name,
					NextRun = e.CurrentWhen + e.Length,
					Action = FinishEvent(e.EventId)
				});

				await eventService.UpdateEventPost(e.EventId);
			}
			await context.SaveChangesAsync();

		}

        public async Task CreateDiscordGuildEvent(int id)
        {
			var e = await context.Events.FirstAsync(ee => ee.EventId == id);
			var when = e.NextWhen;
			var dcGuild = discord.Guilds.First(g => g.Id == e.Guild.DiscordGuildId);

			Image? img = null;
			if (e.HasImage)
				img = new Image(Path.Combine(hostingEnvironment.WebRootPath, "img", "Events", e.EventId + ".png"));

			var dcEvent = await dcGuild.CreateEventAsync(
				e.Name, 
				new DateTimeOffset(when), 
				Discord.GuildScheduledEventType.External, 
				Discord.GuildScheduledEventPrivacyLevel.Private, 
				e.Description, 
				new DateTimeOffset(when).Add(e.Length), 
				null, 
				e.Where,
				img);

			e.DiscordEventId = dcEvent.Id;
			await context.SaveChangesAsync();
		}

		public async Task UpdateDiscordGuildEvent(int id)
		{
			var e = await context.Events.FirstAsync(ee => ee.EventId == id);
			var dcGuild = discord.Guilds.First(g => g.Id == e.Guild.DiscordGuildId);
			var dcEvent = await dcGuild.GetEventAsync(e.DiscordEventId);
			await dcEvent.ModifyAsync(dce =>
			{
				dce.Name = e.Name;
				dce.Location = e.Where;
				dce.Description = e.Description;
				if(e.HasImage)
					dce.CoverImage = new Image(Path.Combine(hostingEnvironment.WebRootPath, "img", "Events", e.EventId + ".png"));
				dce.StartTime = new DateTimeOffset(e.NextWhen);
				dce.EndTime = new DateTimeOffset(e.NextWhen + e.Length);
			});
		}

		public async Task DeleteDiscordGuildEvent(int id)
		{
			var e = await context.Events.FirstAsync(ee => ee.EventId == id);
			var dcGuild = discord.Guilds.First(g => g.Id == e.Guild.DiscordGuildId);
			try
			{
				var dEvent = dcGuild.GetEvent(e.DiscordEventId);
				if(dEvent != null)
					await dEvent.DeleteAsync();
			}
			catch (Exception) { }
			e.DiscordEventId = 0;
			await context.SaveChangesAsync();
		}


		private static Action<IServiceProvider> FinishEvent(int eventId)
        {
			return async (services) =>
			{
				using var scope = services.CreateScope();
				using var context = scope.ServiceProvider.GetRequiredService<Context>();
				var moduleSettings = scope.ServiceProvider.GetRequiredService<ModuleSettings>();
				var e = context.Events.Include(e => e.Guild).First(e => e.EventId == eventId);
                Console.WriteLine(DateTime.Now + "\tFinishing event " + e.Name);
				var settings = new SettingsModel(moduleSettings, e.Guild);

				var discord = services.GetRequiredService<DiscordSocketClient>();
				var channel = discord.Guilds.First(g => g.Id == e.Guild.DiscordGuildId).TextChannels.First(c => c.Id == settings.Channel);
				try
				{
					await channel.DeleteMessageAsync(e.DiscordNotificationId);
				}catch(Exception ex)
                { Console.WriteLine(ex); }				
				e.DiscordNotificationId = 0;
				await context.SaveChangesAsync();

				if (e.Repeats)
					await scope.ServiceProvider.GetRequiredService<EventService>().CreateDiscordGuildEvent(e.EventId);
		   	};
		}

        public static Event? NextEvent(Context context)
        {
			return context.Events
				.Where(e => e.When > DateTime.Now || (e.Repeats && (e.Ends == null || e.Ends > DateTime.Now)))
				.ToList()
				.MinBy(e => e.NextWhen);
		}
	}
}
