using Logger.Abstracts;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

public class Connection
{
    private const int WorkTimeEmulated = 10000;
    
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
        
        server.AddConnections(this);
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
            logger.Error($"При обработке клиента произошла ошибка: {ex.Message}");
        }
        finally
        {
            await server.DisconectAsync(this);
        }
    }

    private async Task TryProcessAsync()
    {
        logger.Debug($"Обработка в потоке {Thread.CurrentThread.ManagedThreadId} для клиента {Socket.RemoteEndPoint}"); 

        var message = await ReadAsync();
        logger.Info($"Получено сообщение: {message} | От {Socket.RemoteEndPoint}");
        message = Reverse(message) + " Сервер напиан Ильиным Артёмом Александровичем. М3О-107Б-23";
        logger.Info($"Отправляю сообдщение: {message} | Кому: {Socket.RemoteEndPoint}");

        Thread.Sleep(WorkTimeEmulated);
        await SendAsync(message);
    }

    private async Task<string> ReadAsync()
    {
        var length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(await ReadBytesAsync(sizeof(int)), 0));
        return Encoding.UTF8.GetString(await ReadBytesAsync(length));
    }

    private async Task<byte[]> ReadBytesAsync(int size)
    {
        var buffer = new byte[size];
        int alreadyReadCount = 0;

        while (size - alreadyReadCount > 0)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(alreadyReadCount, size - alreadyReadCount));
            if (read == 0)
                throw new EndOfStreamException();
            
            alreadyReadCount += read;
        }

        return buffer;
    }

    private string Reverse(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
    
    // По хорошему, надо передавть данные, указывая их размер,
    // но мне лень переделывать код клиента для грамотного считывания.
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
}