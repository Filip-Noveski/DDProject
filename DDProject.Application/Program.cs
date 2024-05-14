using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DDProject.Application;

internal class Program
{
    static async Task Listen(string url, int port)
    {
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(url);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        using Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new(ipAddress, port);
        socket.Bind(endPoint);
        socket.Listen(port);

        Socket handler = await socket.AcceptAsync();
        while (true)
        {
            var buffer = new byte[1_024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            var eom = "<|EOM|>";
            if (response.IndexOf(eom) > -1 /* is end of message */)
            {
                Console.WriteLine(
                    $"Socket server received message: \"{response.Replace(eom, "")}\"");

                var ackMessage = "<|ACK|>";
                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                await handler.SendAsync(echoBytes, 0);
                Console.WriteLine(
                    $"Socket server sent acknowledgment: \"{ackMessage}\"");

                break;
            }
        }
    }

    static async Task Send(string url, int port, string message)
    {
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(url);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        using Socket client = new(SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new(ipAddress, port);
        await client.ConnectAsync(endPoint);

        while (true)
        {
            // Send message.
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
            Console.WriteLine($"Socket client sent message: \"{message}\"");

            // Receive ack.
            var buffer = new byte[1_024];
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response == "<|ACK|>")
            {
                Console.WriteLine(
                    $"Socket client received acknowledgment: \"{response}\"");
                break;
            }
        }

        client.Shutdown(SocketShutdown.Both);
    }

    static async Task Main(string[] args)
    {
        string url = args[0];
        int port = int.Parse(args[1]);

        while (true)
        {
            string op = Console.ReadLine()!;

            if (op == "listen")
            {
                await Listen(url, port);
                continue;
            }

            if (op == "send")
            {
                string targetUrl = Console.ReadLine()!;
                int targetPort = int.Parse(Console.ReadLine()!);
                string message = Console.ReadLine()!;
                await Send(targetUrl, targetPort, message);
                continue;
            }

            if (op == "exit")
            {
                return;
            }
        }
    }
}
