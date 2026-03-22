using System.Net.WebSockets;
using System.Text;

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("<< To connect to the server, use the following command: broadcast-server connect >>");
Console.ResetColor();
string? command = Console.ReadLine();

if (command?.Trim().ToLower() == "broadcast-server connect")
{
    await ConnectToServer();
}
else
{
    Console.WriteLine("Unknown command. Exiting.");
}

async Task ConnectToServer()
{
    Console.Write("👤 Enter your name: ");
    string userName = Console.ReadLine() ?? "Anonymous";

    using var client = new ClientWebSocket();

    try
    {
        await client.ConnectAsync(new Uri("ws://localhost:5000/chat/"), CancellationToken.None);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ You have logged in as: '{userName}'");
        Console.ResetColor();
    }
    catch
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ You were unable to connect");
        Console.ResetColor();
        return;
    }

    var message = Encoding.UTF8.GetBytes(userName);
    await client.SendAsync(new ArraySegment<byte>(message),
        WebSocketMessageType.Text, true, CancellationToken.None);

    Console.WriteLine("💬 Write messages. Type /exit to exit.\n");

    var receive = Task.Run(async () =>
    {
        var buffer = new byte[4096];

        while (client.State == WebSocketState.Open)
        {
            try
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
                    break;
                }

                string raw = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var parties = raw.Split('|', 2);
                string sender = parties[0];
                string message = parties.Length > 1 ? parties[1] : "";

                Console.ForegroundColor = sender == "System" ? ConsoleColor.Yellow
                                        : sender == userName ? ConsoleColor.Cyan
                                        : ConsoleColor.White;

                Console.WriteLine($"  [{sender}]: {message}");
                Console.ResetColor();
            }
            catch
            {
                break;
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n🔌 Connection closed by the server.");
        Console.ResetColor();
    });

    while (client.State == WebSocketState.Open)
    {
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;

        if (input == "/exit")
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Leaving", CancellationToken.None);
            Console.WriteLine("👋 ¡Good bye!");
            break;
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        await client.SendAsync(new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text, true, CancellationToken.None);
    }

    await receive;
}