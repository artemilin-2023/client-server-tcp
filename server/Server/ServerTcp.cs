using System.Net;
using System.Net.Sockets;
using System.Text;
using Server.Logger;

namespace Server;

public class ServerTcp
{
    private const int ServerWorkTime = 10000;
    
    private static ServerTcp? instance;
    private readonly ILogger logger;
    private readonly IPEndPoint endPoint;
    private readonly object locker = new();

    private ServerTcp(ILogger logger, IPEndPoint endPoint)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(endPoint);

        this.logger = logger;
        this.endPoint = endPoint;
    }

    public static ServerTcp GetOrCreate(IPEndPoint endPoint, ILogger logger)
    {
        if (instance != null)
            return instance;

        instance = new ServerTcp(logger, endPoint);
        return instance;
    }

    public void Run()
    {
        lock (locker)
        {
            using var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            socket.Bind(endPoint);
            logger.Info($"Server run on {socket.LocalEndPoint}");
            socket.Listen();
            logger.Info($"The server is now listening on port {socket.LocalEndPoint}");

            using var client = socket.Accept();
            logger.Info($"Connected client address is {client.RemoteEndPoint}");

            using var stream = new NetworkStream(client);
            var message = GetMessageFromStream(stream);
            logger.Info($"Get message is {message}. From {client.RemoteEndPoint}");
            
            message = Reverse(message) + "Сервер написан Ильиным Артёмом Александровичем. Группа: М3О-107Б-23";
            Thread.Sleep(ServerWorkTime);
            SendMessage(stream, message);
            logger.Info($"Send message to {client.RemoteEndPoint}, message is {message}");
        }
    }

    private string GetMessageFromStream(NetworkStream stream)
    {
        if (!stream.CanRead)
            throw new Exception("Невозможно прочитать данные из потока.");
        
        var readBuffer = new byte[512];
        var result = new StringBuilder();
        
        while (stream.DataAvailable)
        {
            var bytesCount = stream.Read(readBuffer);
            result.Append(Encoding.UTF8.GetString(readBuffer, 0, bytesCount));
        }

        return result.ToString();
    }

    private string Reverse(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    private void SendMessage(NetworkStream stream, string message)
    {
        var messageInBytes = Encoding.UTF8.GetBytes(message);
        stream.Write(messageInBytes);
    }
}