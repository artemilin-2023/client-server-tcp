using Logger.Abstracts;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

public class Connection
{
    private const int WorkTimeEmulated = 10000;
    private const int BufferSize = 512;
    
    internal readonly Guid Id;
    internal readonly Socket Socket;
    
    private readonly ServerTcp server;
    private readonly ILogger logger;
    private readonly NetworkStream stream;

    public Connection(Socket socket, ServerTcp server)
    {
        ArgumentNullException.ThrowIfNull(socket);
        ArgumentNullException.ThrowIfNull(server);
        
        Id = Guid.NewGuid();
        Socket = socket;
        this.server = server;
        logger = server.Logger;
        stream = new NetworkStream(socket);
        
        server.AddConnection(this);
        logger.Info($"Новое соединение: {socket.RemoteEndPoint}");
    }

    public async Task ProcessAsync()
    {
        try
        {
            await TryProcessAsync();
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"При обработке клиента произошла ошибка: {ex.Message}");
        }
        finally
        {
            await server.DisconectAsync(this);
        }
    }

    private async Task TryProcessAsync()
    {
        await logger.DebugAsync($"Обработка в потоке {Thread.CurrentThread.ManagedThreadId} для клиента {Socket.RemoteEndPoint}"); 

        var message = await ReadAsync();
        await logger.InfoAsync($"Получено сообщение: {message} | От {Socket.RemoteEndPoint}");
        message = Reverse(message) + " Сервер написан Ильиным Артёмом Александровичем. М3О-107Б-23";
        await logger.InfoAsync($"Отправляю сообщение: {message} | Кому: {Socket.RemoteEndPoint}");

        Thread.Sleep(WorkTimeEmulated);
        await SendAsync(message);
    }

    private async Task<string> ReadAsync()
    {
        var buffer = new byte[BufferSize];
        await stream.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
    }

    private string Reverse(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
    
    private async Task SendAsync(string message)
    {
        var messageInBytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(messageInBytes.AsMemory());
    }

    internal void Close()
    {
        stream.Close();
        Socket.Close();
    }

    // Так как это задание необходимо будет выполнять в паре, и, скорее всего,
    // никто не сделает отправку данных нормальным способом с указанием длины, я буду читать/отправлять данные "как есть"
    //private async Task<string> ReadAsync()
    //{
    //    var length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(await ReadBytesAsync(sizeof(int)), 0));
    //    return Encoding.UTF8.GetString(await ReadBytesAsync(length));
    //}

    //private async Task<byte[]> ReadBytesAsync(int size)
    //{
    //    var buffer = new byte[size];
    //    int alreadyReadCount = 0;

    //    while (size - alreadyReadCount > 0)
    //    {
    //        var read = await stream.ReadAsync(buffer.AsMemory(alreadyReadCount, size - alreadyReadCount));
    //        if (read == 0)
    //            throw new EndOfStreamException();

    //        alreadyReadCount += read;
    //    }

    //    return buffer;
    //}
}