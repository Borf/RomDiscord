using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using RomDiscord.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<CommandHandlingService>();
builder.Services.AddHttpClient();
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
		options.ClaimActions.Add(new JsonSubKeyClaimAction("id", "int", "user", "id"));
		options.ClaimActions.Add(new JsonSubKeyClaimAction("username", "string", "user","username"));
		options.ClaimActions.Add(new JsonSubKeyClaimAction("discriminator", "int", "user","discriminator"));
		options.Events = new OAuthEvents
		{
			OnCreatingTicket = async context =>
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
		};
	});
var app = builder.Build();

//discord connection
var client = app.Services.GetRequiredService<DiscordSocketClient>();
client.Log += LogAsync;
app.Services.GetRequiredService<CommandService>().Log += LogAsync;

await client.LoginAsync(TokenType.Bot, builder.Configuration["Token"]);
await client.StartAsync();
await app.Services.GetRequiredService<CommandHandlingService>().InitializeAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

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