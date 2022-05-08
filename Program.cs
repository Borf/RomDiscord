using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using RomDiscord.Models;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using RomDiscord.Util;
using ServiceStack.Script;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

/*var ctx = new ScriptContext
{
	ScriptMethods = {

		new ProtectedScripts()
	}
};
ctx.Init();
var output = ctx.RenderScript("The time is now: {{ now |> dateFormat('HH:mm:ss') }}", new Dictionary<string, object>()
{
	{"test", "yo" }
});
Console.WriteLine(output);*/



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
DiscordSocketConfig _socketConfig = new()
{
	GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
	AlwaysDownloadUsers = true,
//	LogLevel = LogSeverity.Debug
};
builder.Services.AddDbContext<Context>();
builder.Services.AddSingleton(_socketConfig);
builder.Services.AddHostedService<RomDiscord.Services.TaskScheduler>();
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddScoped<GodEquipRaffle>();
builder.Services.AddScoped<ModuleSettings>();
//builder.Services.AddSingleton<CommandHandlingService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IDistributedCache, DiskCache>();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(60);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = "Discord";
	})
	.AddCookie()
	.AddOAuth("Discord", OAuthHandler);
app = builder.Build();

using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetRequiredService<Context>())
{
//	context.Database.EnsureDeleted();
	if(context.Database.EnsureCreated())
	{
		context.GodEquips.AddRange(
			new GodEquip { Name = "Eternal Spear", Type = "weapon" },
			new GodEquip { Name = "Tyrfing", Type = "weapon" },
			new GodEquip { Name = "God's Gaze", Type = "weapon" },
			new GodEquip { Name = "Golden Bough", Type = "weapon" },
			new GodEquip { Name = "Mystletainn", Type = "weapon" },
			new GodEquip { Name = "All in Peace", Type = "weapon" },
			new GodEquip { Name = "Winter Crystal", Type = "weapon" },
			new GodEquip { Name = "Mjolnir", Type = "weapon" },
			new GodEquip { Name = "War Ender", Type = "weapon" },
			new GodEquip { Name = "Heavenly Queen's Wail", Type = "weapon" },
			new GodEquip { Name = "All Ghosts Seal", Type = "weapon" },
			new GodEquip { Name = "Destroyer", Type = "weapon" },
			new GodEquip { Name = "Warrior's Sunrise", Type = "weapon" },
			new GodEquip { Name = "Whisper of the Moon God", Type = "weapon" },
			new GodEquip { Name = "Omniscience Grimoire", Type = "weapon" },
			new GodEquip { Name = "Silver Hideous Star", Type = "weapon" },
			new GodEquip { Name = "Yoperays Dragon Cannon", Type = "weapon" },
			new GodEquip { Name = "Sun Hat", Type = "hat" },
			new GodEquip { Name = "Moon Hat", Type = "hat" },
			new GodEquip { Name = "Star Hat", Type = "hat" },
			new GodEquip { Name = "Sun Back", Type = "back" },
			new GodEquip { Name = "Moon Back", Type = "back" },
			new GodEquip { Name = "Star Back", Type = "back" }
		);
		await context.SaveChangesAsync();
	}

	//discord connection
	var client = app.Services.GetRequiredService<DiscordSocketClient>();
	client.Log += LogAsync;
	//app.Services.GetRequiredService<CommandService>().Log += LogAsync;
	await app.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
	await client.LoginAsync(TokenType.Bot, builder.Configuration["Token"]);
	await client.StartAsync();
	
	//wait for ready
	TaskCompletionSource readyWaiterCompletion = new TaskCompletionSource();
	var readyWaiter = () => { readyWaiterCompletion.SetResult(); return Task.CompletedTask; };
	client.Ready += readyWaiter;
	await readyWaiterCompletion.Task;
	client.Ready -= readyWaiter;

	//non-interaction based commands
	//await app.Services.GetRequiredService<CommandHandlingService>().InitializeAsync();


	await scope.ServiceProvider.GetRequiredService<GodEquipRaffle>().Initialize();
}




await (app.Services.GetServices<IHostedService>().First(s => (s as RomDiscord.Services.TaskScheduler) != null) as RomDiscord.Services.TaskScheduler).InitializeAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy(new CookiePolicyOptions()
{
	MinimumSameSitePolicy = SameSiteMode.Lax
});
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


void OAuthHandler(OAuthOptions options)
{
	options.ClientId = builder.Configuration["OAuth:Discord:ClientId"];
	options.ClientSecret = builder.Configuration["OAuth:Discord:ClientSecret"];
	options.CallbackPath = new PathString("/LoginDiscord");
	options.AuthorizationEndpoint = builder.Configuration["OAuth:Discord:AuthorizeUrl"];
	options.TokenEndpoint = builder.Configuration["OAuth:Discord:TokenUrl"];
	options.UserInformationEndpoint = builder.Configuration["OAuth:Discord:UserUrl"];
	options.Scope.Add("identify");
	options.Scope.Add("guilds");
	options.ClaimActions.Add(new JsonSubKeyClaimAction("id", "int", "user", "id"));
	options.ClaimActions.Add(new JsonSubKeyClaimAction("username", "string", "user", "username"));
	options.ClaimActions.Add(new JsonSubKeyClaimAction("discriminator", "int", "user", "discriminator"));
	options.Events = new OAuthEvents
	{
		OnCreatingTicket = async context =>
		{
			var user = await getApi(context.Options.UserInformationEndpoint, context);
			/// data.user.id [long]
			/// data.user.username [string]
			/// data.user.avatar [hash]
			/// data.user.discriminator [int]
			context.RunClaimActions(user);
			var guilds = await getApi("https://discord.com/api/users/@me/guilds", context);
			SessionData? sessionData = context.HttpContext.Session.Get<SessionData>("Data");
			if (sessionData == null)
				sessionData = new SessionData();
			sessionData.Guilds.Clear();
			foreach (var guild in guilds.EnumerateArray())
			{
				if ((guild.GetProperty("permissions").GetInt32() & ((int)DiscordGuildPermissions.ADMINISTRATOR)) != 0)
				{
					var guildId = ulong.Parse(guild.GetProperty("id").GetString());

					sessionData.Guilds.Add(new SessionData.SessionGuild()
					{
						Id = guildId,
						Name = guild.GetProperty("name").GetString(),
						Icon = guild.GetProperty("icon").GetString(),
					});
				}
			}
			context.HttpContext.Session.Set("Data", sessionData);

			using var scope = app.Services.CreateScope();
			using var db = scope.ServiceProvider.GetRequiredService<Context>();
			var u = db.Users.FirstOrDefault(u => u.DiscordUserId == ulong.Parse(user.GetProperty("user").GetProperty("id").GetString()));
			if(u == null)
			{
				db.Users.Add(new User()
				{
					DiscordUserId = ulong.Parse(user.GetProperty("user").GetProperty("id").GetString()),
					UserName = user.GetProperty("user").GetProperty("username").GetString() + "#" + user.GetProperty("user").GetProperty("discriminator").GetString()
				});
				db.SaveChanges();
			}
		}
	};
}


Task LogAsync(LogMessage log)
{
    Console.WriteLine(log.ToString());
	return Task.CompletedTask;
}


async Task<JsonElement> getApi(string endPoint, OAuthCreatingTicketContext context)
{
	var request = new HttpRequestMessage(HttpMethod.Get, endPoint);
	request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
	request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
	var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
	response.EnsureSuccessStatusCode();

	return await response.Content.ReadFromJsonAsync<JsonElement>();
}

public partial class Program
{
	private static WebApplication? app;
}