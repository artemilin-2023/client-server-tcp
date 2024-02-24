using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Server.Logger;

namespace Server;

public class ServerTcp
{
    public bool IsRunning { get; private set; } = false;
    
    private const int ServerWorkTime = 10000;
    
    private static ServerTcp? instance;

    private bool IsConfigured = false;
    
    private readonly ILogger logger;
    private readonly IPEndPoint endPoint;
    private readonly Socket server;
    
    private readonly object locker = new();

    private ServerTcp(ILogger logger, IPEndPoint endPoint, Socket server)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(endPoint);
        ArgumentNullException.ThrowIfNull(server);

        this.logger = logger;
        this.endPoint = endPoint;
        this.server = server;
    }

    public static ServerTcp GetOrCreate(string ip, int port, ILogger logger)
    {
        if (instance != null)
            return instance;
        
        var endPoint = new IPEndPoint(IPAddress.Parse(ip), port); 
        var server = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        instance = new ServerTcp(logger, endPoint, server);
        return instance;
    }

    public void Init()
    {
        if (!IsConfigured)
        {
            server.Bind(endPoint);
            logger.Info($"Сервер запущен на {server.LocalEndPoint}");
            server.Listen();
            logger.Info($"Сервер прослушивает сокет {server.LocalEndPoint}");
            
            IsConfigured = true;
        }
        else
        {
            logger.Info("Сервер уже настроен.");
        }
    }

    public void Run()
    {
        if (!IsConfigured)
            throw new Exception("The server must be configured before running");
        
        if (IsRunning)
        {
            logger.Warn("Сервер уже запущен.");
            return;
        }

        IsRunning = true;
        while (IsRunning)
        {
            lock (locker)
            {
                var timer = new Stopwatch();
                timer.Start();
                
                using var client = server.Accept();
                var clientAddress = client.RemoteEndPoint;
                logger.Info($"Новое подсоединение: {clientAddress}");

                using var stream = new NetworkStream(client);
                var message = GetMessageFromStream(stream);
                logger.Info($"Полученное сообщение: {message}. От {clientAddress}");

                message = Reverse(message) + " Сервер написан Ильиным Артёмом Александровичем. Группа: М3О-107Б-23";
                Thread.Sleep(ServerWorkTime);
                SendMessage(stream, message);
                logger.Info($"Отпрака сообщения клиенту с адресом {clientAddress}, текст сообщения: {message}");
                
                client.Close();
                logger.Info($"Клиент {clientAddress} был отключен.");
                
                timer.Stop();
                logger.Debug($"Время, затраченное на обработку клиента: {timer.Elapsed:c}");
            }
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