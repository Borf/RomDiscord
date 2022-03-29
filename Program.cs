using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RomDiscord.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<CommandHandlingService>();
builder.Services.AddHttpClient();
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