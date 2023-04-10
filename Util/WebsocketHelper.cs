using System.Net.WebSockets;

namespace RomDiscord.Util;

public class WebSocketHelper
{
    private string url;
    private ClientWebSocket ws = new ClientWebSocket();

    public event Func<string, Task>? OnData;
    public event Func<Task>? OnConnect;

    public WebSocketHelper(string url)
    {
        this.url = url;
    }



    public async Task Handle()
    {
        while (true) //running
        {
            ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(new Uri(url), CancellationToken.None);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not connect to websocket " + url);
                await Task.Delay(5000);
                continue;
            }
            Console.WriteLine("API Event listener connected to websocket");
            byte[] tmpBuffer = new byte[1024];
            if (OnConnect != null)
                await OnConnect();
            while (ws.State == WebSocketState.Open)
            {
                byte[] buffer = new byte[0];
                try
                {
                    while (true)
                    {
                        var res = await ws.ReceiveAsync(tmpBuffer, CancellationToken.None);
                        var newBuffer = new byte[buffer.Length + res.Count];
                        buffer.CopyTo(newBuffer, 0);
                        tmpBuffer[0..res.Count].CopyTo(newBuffer, buffer.Length);
                        buffer = newBuffer;
                        if (res.EndOfMessage)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    break;
                }
                if (buffer.Length > 0)
                {
                    var text = System.Text.Encoding.UTF8.GetString(buffer);
                    if (OnData != null)
                        await OnData(text);
                }

            }
            Console.WriteLine("SearchAPI disconnected from websocket");
            await Task.Delay(5000);
        }
    }

    internal async Task SendAsync(string data)
    {
        await ws.SendAsync(System.Text.Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
