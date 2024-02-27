using System.Net;
using Logger;
using Microsoft.Extensions.Configuration;
using Server;

# if DEBUG
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.development.json");
var configuration = builder.Build();
# endif

# if (!DEBUG)
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");
var configuration = builder.Build();
# endif

var logger = new LoggerFabric()
    .SetJsonConfiguration("appsettings.json")
    .Build();

logger.Debug("Run program.");

var ip = configuration["Server:IPv4"]!;
var port = configuration.GetValue<int>("Server:Port");
var server = ServerTcp.GetOrCreate(ip, port, logger);

try
{
    server.Init();
    server.Run();
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}
finally
{
    server.Stop();
    logger.Info("Сервер был остановлен.");
}
logger.Debug("The program is completed.");

#if DEBUG
Console.ReadLine();
#endif