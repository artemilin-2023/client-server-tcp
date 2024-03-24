using Logger.Abstracts;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Server;


public class ServerTcp
{
    public bool IsRunning { get; private set; } = false;
    
    internal readonly ILogger Logger;
    
    private static ServerTcp? instance;
    private bool isConfigured = false;
    private readonly IPEndPoint endPoint;
    private readonly Socket server;
    private readonly List<Connection> connections;
    private readonly object locker = new();

    private ServerTcp(ILogger logger, IPEndPoint endPoint, Socket server)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(endPoint);
        ArgumentNullException.ThrowIfNull(server);

        Logger = logger;
        this.endPoint = endPoint;
        this.server = server;
        connections = new List<Connection>();
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

    public async Task Init()
    {
        if (!isConfigured)
        {
            server.Bind(endPoint);
            await Logger.InfoAsync($"Сервер запущен на {server.LocalEndPoint}");
            server.Listen();
            await Logger.InfoAsync($"Сервер прослушивает сокет {server.LocalEndPoint}");
            
            isConfigured = true;
        }
        else
        {
            await Logger.InfoAsync("Сервер уже настроен.");
        }
    }

    public async Task RunAsync()
    {
        if (!isConfigured)
            throw new Exception("The server must be configured before running");

        if (IsRunning)
        {
            await Logger.WarnAsync("Сервер уже запущен.");
            return;
        }

        IsRunning = true;
        while (IsRunning)
        {
            var timer = new Stopwatch();
            timer.Start();

            var client = await server.AcceptAsync();
            var connection = new Connection(client, this); // Создает объект клиента и добавляет его в коллекцию соединений
            var process = new Thread(async () => await connection.ProcessAsync());
            process.Start();
            

            timer.Stop();
            await Logger.DebugAsync($"Время, затраченное на обработку клиента: {timer.Elapsed:c}");

        }

    }

    public void Stop()
    {
        if (!IsRunning)
            throw new Exception("The server is not running");
        
        foreach (var connection in connections)
        {
            connection.Close();
        }
        
        server.Close();
        IsRunning = false;
    }

    internal void AddConnection(Connection connection)
    {
        lock (locker)
        {
            connections.Add(connection);
        }
    }

    internal async Task DisconectAsync(Connection connection)
    {
        await Logger.InfoAsync($"Закрываю соединение с {connection.Socket.RemoteEndPoint}");

        lock (locker)
        {
            connections.Remove(connection);
            connection.Close();
        }
    }
}