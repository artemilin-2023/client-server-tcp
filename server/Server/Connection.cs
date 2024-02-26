using Server.Logger;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

public class Connection
{
    private const int WorkTimeEmulated = 10000;
    
    internal readonly Guid Id;
    
    private readonly Socket socket;
    private readonly ServerTcp server;
    private readonly ILogger logger;
    private readonly NetworkStream stream;

    public Connection(Socket socket, ServerTcp server)
    {
        ArgumentNullException.ThrowIfNull(socket);
        ArgumentNullException.ThrowIfNull(server);
        
        Id = Guid.NewGuid();
        this.socket = socket;
        this.server = server;
        logger = server.Logger;
        stream = new NetworkStream(socket);
        
        server.AddConnections(this);
        logger.Info($"Новое соединение: {socket.RemoteEndPoint}");
    }

    public void Process()
    {
        var address = socket.RemoteEndPoint;
        
        var message = ReadMessage();
        logger.Info($"Получено сообщение: {message} | От {address}");
        message = Reverse(message) + " Сервер напиан Ильиным Артёмом Александровичем. М3О-107Б-23";
        logger.Info($"Отправляю сообдщение: {message} | Кому: {address}");
        
        Thread.Sleep(WorkTimeEmulated);
        SendMessage(message);

        server.Disconect(Id);
        logger.Info($"Соединение с {address} разорвано.");
    }

    private string ReadMessage()
    {
        var length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ReadBytes(sizeof(int)), 0));
        return Encoding.UTF8.GetString(ReadBytes(length));
    }

    private byte[] ReadBytes(int size)
    {
        var buffer = new byte[size];
        int alreadyReadCount = 0;

        while (size - alreadyReadCount > 0)
        {
            var read = stream.Read(buffer, alreadyReadCount,   size - alreadyReadCount);
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
    private void SendMessage(string message)
    {
        var messageInBytes = Encoding.UTF8.GetBytes(message);
        stream.Write(messageInBytes);
    }

    internal void Close()
    {
        stream.Close();
        socket.Close();
    }
}