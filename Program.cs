using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using RomDiscord.Models;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using RomDiscord.Util;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
DiscordSocketConfig _socketConfig = new()
{
	GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
	AlwaysDownloadUsers = true
};
builder.Services.AddDbContext<Context>();
builder.Services.AddSingleton(_socketConfig);
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<InteractionHandler>();
//builder.Services.AddSingleton<CommandHandlingService>();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
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
	.AddOAuth("Discord", options =>
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
		options.ClaimActions.Add(new JsonSubKeyClaimAction("username", "string", "user","username"));
		options.ClaimActions.Add(new JsonSubKeyClaimAction("discriminator", "int", "user","discriminator"));
		options.Events = new OAuthEvents
		{
			OnCreatingTicket = async context =>
			{
				{
					var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
					var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
					response.EnsureSuccessStatusCode();

					var user = await response.Content.ReadFromJsonAsync<JsonElement>();
					/// data.user.id [long]
					/// data.user.username [string]
					/// data.user.avatar [hash]
					/// data.user.discriminator [int]
					context.RunClaimActions(user);
				}
				{
					var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me/guilds");
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
					var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
					response.EnsureSuccessStatusCode();

					SessionData? sessionData = context.HttpContext.Session.Get<SessionData>("Data");
					if (sessionData == null)
						sessionData = new SessionData();
					sessionData.Guilds.Clear();
					var guilds = await response.Content.ReadFromJsonAsync<JsonElement>();
					foreach (var guild in guilds.EnumerateArray())
					{
						if ((guild.GetProperty("permissions").GetInt32() & ((int)DiscordGuildPermissions.ADMINISTRATOR)) != 0)
							sessionData.Guilds.Add(new SessionData.SessionGuild()
							{
								Id = long.Parse(guild.GetProperty("id").GetString()),
								Name = guild.GetProperty("name").GetString(),
								Icon = guild.GetProperty("icon").GetString()
							});
					}
					context.HttpContext.Session.Set("Data", sessionData);
				}

			}
		};
	});
var app = builder.Build();

//discord connection
var client = app.Services.GetRequiredService<DiscordSocketClient>();
client.Log += LogAsync;
//app.Services.GetRequiredService<CommandService>().Log += LogAsync;

await app.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
await client.LoginAsync(TokenType.Bot, builder.Configuration["Token"]);
await client.StartAsync();
//await app.Services.GetRequiredService<CommandHandlingService>().InitializeAsync();

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


Task LogAsync(LogMessage log)
{
    Console.WriteLine(log.ToString());

    return Task.CompletedTask;
}