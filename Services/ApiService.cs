using Discord.WebSocket;
using RomDiscord.Models.Db;
using RomDiscord.Util;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RomDiscord.Services;

public class ApiService
{
    private IServiceProvider services;

    public WebSocketHelper Socket { get; set; }
    public Task ListenTask { get; set; }
    public ApiService(IServiceProvider services)
    {
        this.services = services;
        Socket = new WebSocketHelper("ws://romapi.borf.nl/eu/ChangeListener");
        Socket.OnData += OnData;
        Socket.OnConnect += OnConnect;
        ListenTask = Task.Run(async () => await Socket.Handle());
    }

    private async Task OnConnect()
    {
        await Socket.SendAsync("GuildMove,MvpHunt");
    }

    private async Task OnData(string arg)
    {
        
        var update = JsonSerializer.Deserialize<JsonObject>(arg);
        if (update["Event"].GetValue<string>() == "MvpHunt")
        {
            using var scope = services.CreateScope();
            var huntService = services.GetRequiredService<MvpHuntService>();
            await huntService.Update(); // just update all for now
        }

    }
}