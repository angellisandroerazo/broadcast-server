using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

var clients = new ConcurrentDictionary<string, (WebSocket ws, string nombre)>();

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("<< To start the server, , use the following command: broadcast-server start >>");
Console.ResetColor();
string? command = Console.ReadLine();

if (command?.Trim().ToLower() == "broadcast-server start")
{
    await StartServer();
}
else
{
    Console.WriteLine("Unknown command. Exiting.");
}


async Task StartServer()
{
    HttpListener listener = new HttpListener();
    listener.Prefixes.Add("http://localhost:5000/");
    listener.Start();
    Console.WriteLine("Server started. Listening on http://localhost:5000/");

    while (true)
    {
        HttpListenerContext context = await listener.GetContextAsync();
        HttpListenerRequest request = context.Request;
        if (request.IsWebSocketRequest)
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            _ = Task.Run(() => ReadWebSocket(webSocketContext.WebSocket));
            Console.WriteLine("WebSocket connection established.");
        }
        else
        {
            context.Response.StatusCode = 400;
            context.Response.Close();
        }
    }
}



async Task ReadWebSocket(WebSocket webSocket)
{
    var ws = webSocket;
    var id = Guid.NewGuid().ToString();
    var buffer = new byte[4096];
    string userName = "Anónimo";

    try
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        userName = Encoding.UTF8.GetString(buffer, 0, result.Count).Trim();

        clients[id] = (ws, userName);
        Console.WriteLine($"[{userName}] connected. Clients: {clients.Count}");

        await ServerResponse("System", $"[{userName}] join to chat.", excludeId: id);

        while (ws.State == WebSocketState.Open)
        {
            result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, result.Count).Trim();
            Console.WriteLine($"[{userName}]: {message}");

            await ServerResponse(userName, message, excludeId: null);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: [{userName}]: {ex.Message}");
    }
    finally
    {
        clients.TryRemove(id, out _);
        Console.WriteLine($"[{userName}] deconnected. Total: {clients.Count}");
        await ServerResponse("System", $"{userName} deconnected.", excludeId: id);
    }
}

async Task ServerResponse(string sender, string message, string? excludeId)
{
    var data = Encoding.UTF8.GetBytes($"{sender}|{message}");
    var segment = new ArraySegment<byte>(data);

    var tasks = clients.Where(c => c.Key != excludeId && c.Value.ws.State == WebSocketState.Open)
        .Select(c => c.Value.ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None));
    await Task.WhenAll(tasks);
}