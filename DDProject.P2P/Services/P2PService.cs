using System.Net.Sockets;
using System.Net;
using System.Text;

namespace DDProject.P2P.Services;

public class P2PService
{
    private const string EndOfMessage = "<|EOM|>";
    private const string Ack = "<|ACK|>";

    private readonly string _address;
    private readonly int _port;

    public P2PService(string address, int port)
    {
        _address = address;
        _port = port;
    }

    public async Task<string> Listen()
    {
        StringBuilder sb = new();
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(_address);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        using Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new(ipAddress, _port);
        socket.Bind(endPoint);
        socket.Listen(_port);

        Socket handler = await socket.AcceptAsync();
        while (true)
        {
            byte[] buffer = new byte[1_024];
            int received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            sb.Append(response);

            if (response.IndexOf(EndOfMessage) > -1) /* is end of message */
            {
                // return ack
                byte[] echoBytes = Encoding.UTF8.GetBytes(Ack);
                await handler.SendAsync(echoBytes, 0);

                break;
            }
        }

        return sb.ToString();
    }

    public async Task<string> Send(string url, int port, string message)
    {
        StringBuilder sb = new();
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(url);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        using Socket client = new(SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new(ipAddress, port);
        await client.ConnectAsync(endPoint);
        message += EndOfMessage;

        while (true)
        {
            // Send message.
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _ = await client.SendAsync(messageBytes, SocketFlags.None);

            // Receive ack.
            byte[] buffer = new byte[1_024];
            int received = await client.ReceiveAsync(buffer, SocketFlags.None);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response == Ack)
            {
                break;
            }

            // don't append ack
            sb.Append(response);
        }

        client.Shutdown(SocketShutdown.Both);
        return sb.ToString();
    }
}
